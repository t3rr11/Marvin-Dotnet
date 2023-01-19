create function "NotifyOnDataInsert"() returns trigger
    language plpgsql
as
$$
DECLARE
    data JSON;
    notification JSON;
BEGIN
    IF (TG_OP = 'INSERT') THEN
        data = row_to_json(NEW);
    END IF;

    -- create json payload
    -- note that here can be done projection 
    notification = json_build_object(
            'table', TG_TABLE_NAME,
            'action', TG_OP, -- can have value of INSERT, UPDATE, DELETE
            'data', data);

    -- note that channel name MUST be lowercase, otherwise pg_notify() won't work
    PERFORM pg_notify('datainsert', notification::TEXT);
RETURN NEW;
END
$$;

alter function "NotifyOnDataInsert"() owner to terrii;