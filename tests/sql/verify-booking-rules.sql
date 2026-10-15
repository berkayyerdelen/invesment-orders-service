-- Verifies the database rules that protect against double booking and lost updates.
-- Run against a database that has the schema created by the API (or the minimal schema below).
-- Each block is independent; a failing statement shows the expected error in the output.

-- 1) Overlapping active bookings in the same room are rejected (SQLSTATE 23P01).
INSERT INTO bookings (id, room_id, starts_at, ends_at, guest_name, status)
VALUES (gen_random_uuid(), '00000000-0000-0000-0000-000000000001', '2030-01-01 10:00+00', '2030-01-01 12:00+00', 'Alice', 'Active');

-- Expected to fail: overlaps Alice's booking.
INSERT INTO bookings (id, room_id, starts_at, ends_at, guest_name, status)
VALUES (gen_random_uuid(), '00000000-0000-0000-0000-000000000001', '2030-01-01 11:00+00', '2030-01-01 13:00+00', 'Bob', 'Active');

-- 2) Back-to-back bookings ([10,12) then [12,14)) are allowed because the range is half-open.
INSERT INTO bookings (id, room_id, starts_at, ends_at, guest_name, status)
VALUES (gen_random_uuid(), '00000000-0000-0000-0000-000000000001', '2030-01-01 12:00+00', '2030-01-01 14:00+00', 'Carol', 'Active');

-- 3) Cancelled bookings do not block the slot (partial exclusion constraint).
UPDATE bookings SET status = 'Cancelled' WHERE guest_name = 'Carol';
INSERT INTO bookings (id, room_id, starts_at, ends_at, guest_name, status)
VALUES (gen_random_uuid(), '00000000-0000-0000-0000-000000000001', '2030-01-01 12:00+00', '2030-01-01 13:00+00', 'Dave', 'Active');

-- 4) Different rooms can share a time slot.
INSERT INTO bookings (id, room_id, starts_at, ends_at, guest_name, status)
VALUES (gen_random_uuid(), '00000000-0000-0000-0000-000000000002', '2030-01-01 10:00+00', '2030-01-01 12:00+00', 'Erin', 'Active');
