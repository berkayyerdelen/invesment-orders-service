-- Initial schema. Column names match the EF Core mapping in AppDbContext.
-- The optimistic concurrency token is PostgreSQL's xmin system column, so it is not created here.

CREATE TABLE IF NOT EXISTS rooms (
    id   uuid         PRIMARY KEY,
    name varchar(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS bookings (
    id         uuid          PRIMARY KEY,
    room_id    uuid          NOT NULL REFERENCES rooms (id) ON DELETE RESTRICT,
    starts_at  timestamptz   NOT NULL,
    ends_at    timestamptz   NOT NULL,
    guest_name varchar(150)  NOT NULL,
    status     varchar(20)   NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_bookings_room_id_starts_at ON bookings (room_id, starts_at);

-- Database-level rules that EF Core cannot express.
CREATE EXTENSION IF NOT EXISTS btree_gist;

ALTER TABLE bookings
    ADD CONSTRAINT bookings_time_range_valid CHECK (ends_at > starts_at);

-- No two active bookings may overlap in the same room. Half-open ranges ([start, end))
-- allow back-to-back bookings. Cancelled bookings are excluded from the constraint.
ALTER TABLE bookings
    ADD CONSTRAINT bookings_no_overlap
    EXCLUDE USING gist (room_id WITH =, tstzrange(starts_at, ends_at, '[)') WITH &&)
    WHERE (status = 'Active');
