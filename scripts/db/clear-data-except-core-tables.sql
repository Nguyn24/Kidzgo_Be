BEGIN;

DO $$
DECLARE
    tables_to_truncate text;
BEGIN
    SELECT string_agg(format('%I.%I', schemaname, tablename), ', ' ORDER BY tablename)
    INTO tables_to_truncate
    FROM pg_tables
    WHERE schemaname = 'public'
      AND tablename NOT IN (
          'Branches',
          'Users',
          'NotificationTemplates',
          'EmailTemplates',
          '__EFMigrationsHistory'
      );

    IF tables_to_truncate IS NULL THEN
        RAISE NOTICE 'No tables found to truncate.';
        RETURN;
    END IF;

    EXECUTE 'TRUNCATE TABLE ' || tables_to_truncate || ' RESTART IDENTITY';
    RAISE NOTICE 'Truncated tables: %', tables_to_truncate;
END
$$;

COMMIT;
