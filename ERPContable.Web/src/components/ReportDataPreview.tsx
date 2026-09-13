type JsonRecord = Record<string, unknown>;

const isRecord = (value: unknown): value is JsonRecord =>
  typeof value === 'object' && value !== null && !Array.isArray(value);

const formatKey = (key: string) => key.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/[_-]/g, ' ');

const formatValue = (value: unknown) => {
  if (value === null || value === undefined || value === '') return '—';
  if (typeof value === 'number') return new Intl.NumberFormat('es-PE', { maximumFractionDigits: 2 }).format(value);
  if (typeof value === 'boolean') return value ? 'Sí' : 'No';
  return String(value);
};

const primitive = (value: unknown) => value === null || ['string', 'number', 'boolean'].includes(typeof value);

function CellValue({ value }: { value: unknown }) {
  return primitive(value) ? <span className={typeof value === 'number' ? 'numeric' : ''}>{formatValue(value)}</span> : <span className="report-complex-value">Ver detalle</span>;
}

function RecordsTable({ rows }: { rows: JsonRecord[] }) {
  const columns = Array.from(new Set(rows.flatMap((row) => Object.keys(row))));
  return <div className="table-wrap report-data-wrap"><table className="report-data-table"><caption className="sr-only">Datos del reporte consultado</caption><thead><tr>{columns.map((column) => <th key={column} scope="col">{formatKey(column)}</th>)}</tr></thead><tbody>{rows.map((row, index) => <tr key={index}>{columns.map((column) => <td key={column}><CellValue value={row[column]} /></td>)}</tr>)}</tbody></table></div>;
}

function ValueBlock({ value }: { value: unknown }) {
  if (primitive(value)) return <CellValue value={value} />;
  if (Array.isArray(value)) {
    const records = value.length > 0 && value.every(isRecord) ? value : [];
    return records.length > 0 ? <RecordsTable rows={records} /> : <div className="report-value-list">{value.length === 0 ? 'Sin elementos' : value.map((item, index) => <div className="report-value-item" key={index}><span>Registro {index + 1}</span><ValueBlock value={item} /></div>)}</div>;
  }
  if (isRecord(value)) return <div className="report-object">{Object.entries(value).map(([key, item]) => <div className="report-object-row" key={key}><span>{formatKey(key)}</span><ValueBlock value={item} /></div>)}</div>;
  return <CellValue value={value} />;
}

export function ReportDataPreview({ data }: { data: unknown }) {
  if (data === null || data === undefined) return <div className="empty-state">El reporte no contiene datos para el período consultado.</div>;
  if (Array.isArray(data) && data.length > 0 && data.every(isRecord)) return <RecordsTable rows={data} />;
  if (isRecord(data)) return <div className="report-object report-root-object">{Object.entries(data).map(([key, value]) => <div className="report-object-row" key={key}><span>{formatKey(key)}</span><ValueBlock value={value} /></div>)}</div>;
  return <div className="report-scalar"><CellValue value={data} /></div>;
}
