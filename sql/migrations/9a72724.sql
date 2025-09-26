-- Migration, came from 9a72724
BEGIN;
ALTER TABLE IF EXISTS crates_owed
    RENAME COLUMN crates TO cases;

ALTER TABLE IF EXISTS crates_owed
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC');

ALTER TABLE IF EXISTS crates_owed
    RENAME TO cases_owed;
COMMIT;
BEGIN;
ALTER TABLE IF EXISTS crates_given
    RENAME COLUMN crates to cases;

ALTER TABLE IF EXISTS crates_given
    RENAME TO cases_given;
COMMIT;
