BEGIN;

CREATE TABLE IF NOT EXISTS persons (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    discord_id BIGINT UNIQUE NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC')
);

CREATE TABLE IF NOT EXISTS user_sessions (
    id SERIAL PRIMARY KEY,
    person_id INT REFERENCES persons(id) ON DELETE CASCADE,
    session_token UUID DEFAULT gen_random_uuid() UNIQUE NOT NULL,
    created_at TIMESTAMP DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
    expires_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS cases_owed (
    person_id INT PRIMARY KEY REFERENCES persons(id) ON DELETE CASCADE,
    cases FLOAT DEFAULT 1,
    updated_at TIMESTAMP DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC')
);

CREATE TABLE IF NOT EXISTS cases_given (
    person_id INT REFERENCES persons(id) ON DELETE CASCADE,
    given_at TIMESTAMP DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
    cases FLOAT DEFAULT 1,
    PRIMARY KEY (person_id, given_at)
);


-- Time series esque table for tracking beers drank by persons
CREATE TABLE IF NOT EXISTS beers_drank (
    person_id INT REFERENCES persons(id) ON DELETE CASCADE,
    drank_at TIMESTAMP DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
    PRIMARY KEY (person_id, drank_at)
);

COMMIT;