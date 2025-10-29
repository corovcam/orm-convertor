export interface RequiredContentUnit {
  id: number;
  contentKindId: string;
  description: string;
  languages: string[];
}

export interface RequiredContentDefinition {
  ormId: string;
  required: RequiredContentUnit[];
}
