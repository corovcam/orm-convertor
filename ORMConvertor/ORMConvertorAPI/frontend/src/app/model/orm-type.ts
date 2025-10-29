export interface OrmTechnology {
  id: string;
  displayName: string;
  description?: string;
  languages: string[];
  paradigms: string[];
  serializers: string[];
  supportedContentKinds: string[];
}
