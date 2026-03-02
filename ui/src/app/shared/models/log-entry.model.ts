export interface LogEntry {
  id: number;
  category: string;
  logLevel: string;
  message: string;
  exception: string | null;
  timestamp: string;
}
