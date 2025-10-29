
import { SourceUnit } from "./convert";

export interface AdvisorMetricWeights {
  latencyWeight: number;
  memoryWeight: number;
  consistencyWeight: number;
  costWeight: number;
}

export interface AdvisorWorkloadCharacteristics {
  concurrentUsers: number;
  readPercentage: number;
  writePercentage: number;
  consistencyPreference: string;
  metricWeights: AdvisorMetricWeights;
}

export interface AdvisorQueryWorkload {
  workloadCategory?: string | null;
  queryShape?: string | null;
  estimatedResultSetSize?: number;
}

export interface AdvisorRunQuery {
  id: string;
  query: SourceUnit;
  weight: number;
  workload?: AdvisorQueryWorkload | null;
}

export interface AdvisorRunRequest {
  sourceOrmId: string;
  entities: SourceUnit[];
  queries: AdvisorRunQuery[];
  maxMemoryBytes: number;
  maxFrameworksToSelect: number;
  targetFrameworks?: string[];
  workload?: AdvisorWorkloadCharacteristics;
}

export interface AdvisorBenchmarkMetrics {
  latencyMs: number;
  memoryBytes: number;
  consistencyScore: number;
  monetaryCost: number;
  additionalMetrics?: Record<string, number>;
}

export interface AdvisorRunResult {
  objective: number;
  selectedFrameworks: string[];
  queryAssignments: Record<string, string>;
  benchmarkSummaries: Record<string, Record<string, AdvisorBenchmarkMetrics>>;
}

export interface AdvisorFrameworkDescriptor {
  id: string;
  displayName: string;
  category: string;
  tags: string[];
}

export interface AdvisorMetricPreset {
  id: string;
  displayName: string;
  weights: {
    latency: number;
    memory: number;
    consistency: number;
    cost: number;
  };
}
