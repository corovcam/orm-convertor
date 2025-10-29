export interface SourceUnit {
  contentKindId: string;
  language?: string | null;
  content: string;
}

export interface ConvertRequest {
  sourceOrmId: string;
  targetOrmId: string;
  sources: SourceUnit[];
}

export interface ConvertResponse {
  sources: SourceUnit[];
}
