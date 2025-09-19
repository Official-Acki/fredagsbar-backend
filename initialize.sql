CREATE TABLE IF NOT EXISTS persons (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    discord_id BIGINT UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS user_sessions (
    id SERIAL PRIMARY KEY,
    person_id INT REFERENCES persons(id) ON DELETE CASCADE,
    session_token UUID DEFAULT gen_random_uuid() UNIQUE NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS crates_owed (
    person_id INT PRIMARY KEY REFERENCES persons(id) ON DELETE CASCADE,
    crates FLOAT DEFAULT 1
);

CREATE TABLE IF NOT EXISTS crates_given (
    -- Composite primary key user and timestamp
    person_id INT REFERENCES persons(id) ON DELETE CASCADE,
    given_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    crates FLOAT DEFAULT 1,
    PRIMARY KEY (person_id, given_at)
);


-- Time series table for tracking beers drank by persons
CREATE TABLE IF NOT EXISTS beers_drank (
    person_id INT REFERENCES persons(id) ON DELETE CASCADE,
    drank_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (person_id, drank_at)
);



-- Placeholders
INSERT INTO persons (username, discord_id, password_hash) VALUES
('alice', 123456789012345678, 'hashed_password_1'),
('bob', 234567890123456789, 'hashed_password_2'),
('charlie', 345678901234567890, 'hashed_password_3')
ON CONFLICT (username) DO NOTHING;

/*
IF YOU NEED TO RESET THE DATABASE, UNCOMMENT THE FOLLOWING LINES:
DROP TABLE IF EXISTS persons CASCADE;
DROP TABLE IF EXISTS user_sessions CASCADE;
DROP TABLE IF EXISTS crates_owed CASCADE;
DROP TABLE IF EXISTS crates_given CASCADE;
DROP TABLE IF EXISTS beers_drank CASCADE;
*/