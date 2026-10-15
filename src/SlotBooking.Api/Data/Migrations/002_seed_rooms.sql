-- Default rooms. Fixed IDs keep the seed idempotent and easy to reference in tests.
INSERT INTO rooms (id, name) VALUES
    ('0a6f5b3e-1c2d-4e5f-8a9b-000000000001', 'Room A'),
    ('0a6f5b3e-1c2d-4e5f-8a9b-000000000002', 'Room B'),
    ('0a6f5b3e-1c2d-4e5f-8a9b-000000000003', 'Meeting Room 1')
ON CONFLICT (id) DO NOTHING;
