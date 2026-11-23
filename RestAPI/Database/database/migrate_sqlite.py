import os
import sqlite3
from pathlib import Path

import psycopg
from psycopg import sql

ROOT = Path(__file__).resolve().parent.parent
SQLITE_PATH = Path(os.getenv("SQLITE_PATH", ROOT / "DB_N.sqlite"))
SCHEMA_PATH = Path(__file__).with_name("schema.sql")
TABLES = ("Publishers", "Readers", "Books", "Loans", "User")


def main():
    sqlite_db = sqlite3.connect(f"file:{SQLITE_PATH}?mode=ro", uri=True)
    sqlite_db.row_factory = sqlite3.Row

    conninfo = {
        "host": os.getenv("PGHOST", "localhost"),
        "port": os.getenv("PGPORT", "5433"),
        "dbname": os.getenv("PGDATABASE", "LibraryDB"),
        "user": os.getenv("PGUSER", "user"),
        "password": os.environ["DB_PASSWORD"],
    }

    try:
        with psycopg.connect(**conninfo) as pg:
            pg.execute(SCHEMA_PATH.read_text(encoding="utf-8"))
            for table in TABLES:
                rows = sqlite_db.execute(f'SELECT * FROM "{table}"').fetchall()
                if not rows:
                    continue
                columns = rows[0].keys()
                insert = sql.SQL("INSERT INTO {} ({}) VALUES ({}) ON CONFLICT DO NOTHING").format(
                    sql.Identifier(table),
                    sql.SQL(", ").join(map(sql.Identifier, columns)),
                    sql.SQL(", ").join(sql.Placeholder() for _ in columns),
                )
                with pg.cursor() as cursor:
                    cursor.executemany(insert, [tuple(row) for row in rows])
                print(f"{table}: migrated {len(rows)} rows")

            for table, column in (("Publishers", "Publisher_id"), ("Readers", "Reader_id"), ("Loans", "Loan_id")):
                pg.execute(
                    sql.SQL("SELECT setval(pg_get_serial_sequence(format('%I', {}), {}), COALESCE(MAX({}), 1), MAX({}) IS NOT NULL) FROM {}").format(
                        sql.Literal(table),
                        sql.Literal(column),
                        sql.Identifier(column),
                        sql.Identifier(column),
                        sql.Identifier(table),
                    )
                )
    finally:
        sqlite_db.close()


if __name__ == "__main__":
    main()
