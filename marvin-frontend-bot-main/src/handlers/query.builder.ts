const type = (d) => {
  if (Array.isArray(d)) {
    return `'${JSON.stringify(d).split("'").join("''")}'`;
  } else if (!isNaN(d)) {
    if (!(parseInt(d, 10) > 2147483647)) {
      return d;
    } else {
      return `'${d.split("'").join("''")}'`;
    }
  } else if (!isNaN(d) || d === true || d === false) {
    return d;
  } else {
    return `'${d?.split("'")?.join("''")}'`;
  }
};

export const createUpsertQuery = (data: Object, table: string, unique_key: string) => {
  return `
    INSERT INTO ${table}
      (${Object.keys(data).join(', ')})
    VALUES
      (${Object.values(data)
        .map((e) => type(e))
        .join(', ')})
    ON CONFLICT (${unique_key})
    DO UPDATE SET
      ${Object.entries(data)
        .map((column) => `${column[0]} = ${type(column[1])}`)
        .join(',')}
    RETURNING *
  `;
};

export const createUpdateQuery = (data: Object, table: string, where: { column: string; data: any }) => {
  try {
    return `
    UPDATE ${table}
    SET ${Object.entries(data)
      .map((column) => `${column[0]} = ${type(column[1])}`)
      .join(',')}
    WHERE ${where.column} = ${type(where.data)}
    RETURNING *
  `;
  } catch (err) {
    console.log(data, table, where);
    console.error(err);
  }
};

export const insertQuery = (data: Object, table: string) => {
  return `
    INSERT INTO ${table}
      (${Object.keys(data).join(', ')})
    VALUES
      (${Object.values(data)
        .map((e) => type(e))
        .join(', ')})
    RETURNING *
  `;
};
