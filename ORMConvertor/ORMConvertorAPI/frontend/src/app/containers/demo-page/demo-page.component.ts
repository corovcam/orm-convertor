import {
  CommonModule,
  Location,
} from "@angular/common";
import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  HostListener,
  OnInit,
  inject,
} from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormsModule } from "@angular/forms";
import { combineLatest, finalize, of } from "rxjs";
import { delay } from "rxjs/operators";
import { ContentDisplayComponent } from "../../components/content-display/content-display.component";
import { ResultTableComponent } from "../../components/result-table/result-table.component";
import { ContentKind } from "../../model/content-type";
import { ConvertRequest, SourceUnit } from "../../model/convert";
import { OrmTechnology } from "../../model/orm-type";
import {
  AdvisorFrameworkDescriptor,
  AdvisorMetricPreset,
  AdvisorRunRequest,
  AdvisorRunResult,
} from "../../model/advisor";
import {
  RequiredContentDefinition,
  RequiredContentUnit,
} from "../../model/required-content";
import { OrmService } from "../../services/orm.service";
import { SAMPLES } from "../../model/samples";

interface EditableUnitState {
  content: string;
  language: string | null;
}

@Component({
  selector: "app-demo-page",
  standalone: true,
  imports: [CommonModule, FormsModule, ContentDisplayComponent, ResultTableComponent],
  templateUrl: "./demo-page.component.html",
  styleUrls: ["./demo-page.component.less"],
})
export class DemoPageComponent implements OnInit, AfterViewInit {
  private destroyRef = inject(DestroyRef);

  showResults = false;
  isLoading = false;
  loadingDots = "";
  private loadingInterval?: any;

  orms: OrmTechnology[] = [];
  contentKinds = new Map<string, ContentKind>();
  requiredContent: RequiredContentDefinition[] = [];
  displayUnits: RequiredContentUnit[] = [];
  contentState: Record<number, EditableUnitState> = {};

  samples: Map<number, string> = new Map();

  advisorFrameworks: AdvisorFrameworkDescriptor[] = [];
  metricPresets: AdvisorMetricPreset[] = [];
  selectedPresetId: string | null = null;
  metricWeights = {
    latencyWeight: 1,
    memoryWeight: 1,
    consistencyWeight: 1,
    costWeight: 1,
  };

  maxMemoryKb = 2048;
  maxFrameworksToSelect = 2;
  concurrentUsers = 10;
  readPercentage = 70;
  writePercentage = 30;
  consistencyPreference = "balanced";

  queryWeights: Record<number, number> = {};

  advisorIsRunning = false;
  advisorError = "";
  advisorResult: AdvisorRunResult | null = null;

  sourceOrmId: string | null = null;
  targetOrmId: string | null = null;
  selectedTargetOrms: Set<string> = new Set();

  convertedUnits: SourceUnit[] = [];
  error = "";

  constructor(
    private ormService: OrmService,
    private elRef: ElementRef,
    private location: Location
  ) {}

  ngOnInit(): void {
    combineLatest([
      this.ormService.getOrmTechnologies(),
      this.ormService.getContentKinds(),
      this.ormService.getRequiredContentAdvisor(),
      this.ormService.getAdvisorFrameworks(),
      this.ormService.getAdvisorMetricPresets(),
    ])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(([orms, kinds, required, frameworks, presets]) => {
        this.orms = orms;
        this.contentKinds = new Map(kinds.map((kind) => [kind.id, kind]));
        this.requiredContent = required;
        this.advisorFrameworks = frameworks;
        this.metricPresets = presets;

        this.sourceOrmId = this.resolveDefaultSourceOrm();
        this.targetOrmId = this.resolveDefaultTargetOrm();
        this.selectedTargetOrms = new Set(
          (this.advisorFrameworks.length ? this.advisorFrameworks : this.orms)
            .map((framework) => framework.id)
            .filter((id) => id !== this.sourceOrmId)
        );

        this.applyPreset(this.selectedPresetId ?? this.metricPresets[0]?.id ?? null);

        this.updateRequiredUnits();
      });

    this.samples = new Map([
      [4, SAMPLES.entity],
      [5, SAMPLES.query1],
      [6, SAMPLES.query2],
      [7, SAMPLES.query3],
    ]);
  }

  private resolveDefaultSourceOrm(): string | null {
    const preferred = this.orms.find((o) => o.id === "ef-core");
    return preferred?.id ?? this.orms[0]?.id ?? null;
  }

  private resolveDefaultTargetOrm(): string | null {
    const preferred = this.orms.find((o) => o.id === "dapper");
    if (preferred && preferred.id !== this.sourceOrmId) {
      return preferred.id;
    }

    const alternative = this.orms.find((o) => o.id !== this.sourceOrmId);
    return alternative?.id ?? this.sourceOrmId;
  }

  onSourceOrmChange(newId: string) {
    this.sourceOrmId = newId;
    this.updateRequiredUnits();
  }

  onTargetOrmChange(newId: string) {
    this.targetOrmId = newId;
    this.convertedUnits = [];
  }

  onTargetOrmToggle(ormId: string, checked: boolean): void {
    if (checked) {
      this.selectedTargetOrms.add(ormId);
    } else {
      this.selectedTargetOrms.delete(ormId);
    }
  }

  onPresetChange(presetId: string): void {
    this.applyPreset(presetId);
  }

  updateMetricWeight(key: keyof typeof this.metricWeights, value: number): void {
    const parsed = Number.isFinite(value) ? value : 0;
    this.metricWeights = {
      ...this.metricWeights,
      [key]: Math.max(0, parsed),
    };
  }

  updateQueryWeight(unitId: number, value: number): void {
    const parsed = Math.max(1, Math.round(value));
    this.queryWeights = {
      ...this.queryWeights,
      [unitId]: parsed,
    };
  }

  private updateRequiredUnits() {
    if (!this.sourceOrmId) {
      this.displayUnits = [];
      return;
    }

    const match = this.requiredContent.find((r) => r.ormId === this.sourceOrmId);
    this.displayUnits = [...(match?.required ?? [])];

    this.displayUnits.forEach((unit) => {
      const descriptor = this.contentKinds.get(unit.contentKindId);
      const defaultLanguage = descriptor?.defaultLanguage ?? descriptor?.languages[0] ?? null;
      if (!this.contentState[unit.id]) {
        this.contentState[unit.id] = { content: "", language: defaultLanguage };
      } else if (!this.contentState[unit.id].language) {
        this.contentState[unit.id].language = defaultLanguage;
      }
    });
  }

  private applyPreset(presetId: string | null): void {
    if (!presetId) {
      return;
    }

    const match = this.metricPresets.find((preset) => preset.id === presetId);
    if (!match) {
      return;
    }

    this.selectedPresetId = match.id;
    this.metricWeights = {
      latencyWeight: match.weights.latency,
      memoryWeight: match.weights.memory,
      consistencyWeight: match.weights.consistency,
      costWeight: match.weights.cost,
    };
  }

  convert(): void {
    if (!this.sourceOrmId || !this.targetOrmId) {
      return;
    }

    this.isLoading = true;
    this.loadingDots = "";
    this.loadingInterval = setInterval(() => {
      if (this.loadingDots.length < 6) {
        this.loadingDots += ".";
      } else {
        this.loadingDots = "";
      }
    }, 500);

    const body: ConvertRequest = {
      sourceOrmId: this.sourceOrmId,
      targetOrmId: this.targetOrmId,
      sources: this.displayUnits.map((unit) => ({
        contentKindId: unit.contentKindId,
        language:
          this.contentState[unit.id]?.language ??
          this.contentKinds.get(unit.contentKindId)?.defaultLanguage ??
          null,
        content: this.contentState[unit.id]?.content ?? "",
      })),
    };

    this.error = "";
    this.convertedUnits = [];

    of(body)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        delay(2000),
        finalize(() => {
          this.isLoading = false;
          this.showResults = true;
          if (this.loadingInterval) {
            clearInterval(this.loadingInterval);
            this.loadingInterval = undefined;
          }
          this.loadingDots = "";
          setTimeout(() => this.resizeAll(), 0);
        })
      )
      .subscribe({
        next: () => {
          this.convertedUnits = [
            {
              contentKindId: "csharp-entity",
              language: "C#",
              content: SAMPLES.entityTarget,
            },
            {
              contentKindId: "csharp-query",
              language: "C#",
              content: SAMPLES.tquery1,
            },
            {
              contentKindId: "csharp-query",
              language: "C#",
              content: SAMPLES.tquery2,
            },
            {
              contentKindId: "csharp-query",
              language: "C#",
              content: SAMPLES.tquery3,
            },
          ];
        },
        error: (err) => (this.error = err.message ?? "Optimization failed"),
      });
  }

  fillWithSamples(): void {
    this.displayUnits.forEach((unit) => {
      const sample = this.samples.get(unit.id);
      if (sample !== undefined) {
        this.ensureState(unit.id, unit.contentKindId);
        this.contentState[unit.id].content = sample;
      }
    });
    setTimeout(() => this.resizeAll(), 0);
  }

  updateContent(unit: RequiredContentUnit, value: string) {
    this.ensureState(unit.id, unit.contentKindId);
    this.contentState[unit.id].content = value;
  }

  updateLanguage(unit: RequiredContentUnit, language: string) {
    this.ensureState(unit.id, unit.contentKindId);
    this.contentState[unit.id].language = language;
  }

  private isQueryUnit(unit: RequiredContentUnit): boolean {
    return unit.contentKindId.toLowerCase().includes("query");
  }

  private toSourceUnit(unit: RequiredContentUnit): SourceUnit {
    this.ensureState(unit.id, unit.contentKindId);
    const state = this.contentState[unit.id];
    const descriptor = this.contentKinds.get(unit.contentKindId);
    return {
      contentKindId: unit.contentKindId,
      language: state.language ?? descriptor?.defaultLanguage ?? null,
      content: state.content,
    };
  }

  private getQueryUnits(): RequiredContentUnit[] {
    return this.displayUnits.filter((unit) => this.isQueryUnit(unit));
  }

  private buildAdvisorRequest(): AdvisorRunRequest | null {
    if (!this.sourceOrmId) {
      return null;
    }

    const entities = this.displayUnits
      .filter((unit) => !this.isQueryUnit(unit))
      .map((unit) => this.toSourceUnit(unit));

    const queries = this.getQueryUnits().map((unit, index) => ({
      id: String(unit.id ?? index),
      query: this.toSourceUnit(unit),
      weight: this.queryWeights[unit.id] ?? 1,
      workload: {
        workloadCategory: unit.description?.toLowerCase() ?? "default",
        queryShape: unit.description ?? null,
        estimatedResultSetSize: 0,
      },
    }));

    if (queries.length === 0) {
      return null;
    }

    return {
      sourceOrmId: this.sourceOrmId,
      entities,
      queries,
      maxMemoryBytes: Math.max(0, this.maxMemoryKb) * 1024,
      maxFrameworksToSelect: Math.max(1, this.maxFrameworksToSelect),
      targetFrameworks: Array.from(this.selectedTargetOrms),
      workload: {
        concurrentUsers: Math.max(1, this.concurrentUsers),
        readPercentage: Math.max(0, Math.min(100, this.readPercentage)),
        writePercentage: Math.max(0, Math.min(100, this.writePercentage)),
        consistencyPreference: this.consistencyPreference,
        metricWeights: { ...this.metricWeights },
      },
    };
  }

  runAdvisor(): void {
    const request = this.buildAdvisorRequest();
    if (!request) {
      this.advisorError = "Provide entity and query definitions before running the advisor.";
      return;
    }

    this.advisorIsRunning = true;
    this.advisorError = "";
    this.advisorResult = null;

    this.ormService
      .runAdvisor(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.advisorResult = result;
          this.advisorIsRunning = false;
        },
        error: (err) => {
          this.advisorError = err?.message ?? "Advisor run failed";
          this.advisorIsRunning = false;
        },
      });
  }

  getFrameworkDisplayName(id: string | null): string {
    if (!id) {
      return "";
    }

    return (
      this.orms.find((o) => o.id === id)?.displayName ??
      this.advisorFrameworks.find((f) => f.id === id)?.displayName ??
      id
    );
  }

  getQueryWeight(unitId: number): number {
    return this.queryWeights[unitId] ?? 1;
  }

  getQueryKey(unit: RequiredContentUnit): string {
    return String(unit.id);
  }

  private ensureState(unitId: number, contentKindId: string) {
    if (!this.contentState[unitId]) {
      const descriptor = this.contentKinds.get(contentKindId);
      this.contentState[unitId] = {
        content: "",
        language: descriptor?.defaultLanguage ?? descriptor?.languages[0] ?? null,
      };
    }
  }

  getContentKind(id: string): ContentKind | undefined {
    return this.contentKinds.get(id);
  }

  selectAllTargets(): void {
    const frameworks = this.advisorFrameworks.length
      ? this.advisorFrameworks.map((f) => f.id)
      : this.orms.map((o) => o.id);
    this.selectedTargetOrms = new Set(frameworks);
  }

  @HostListener("input", ["$event"])
  onInput(event: Event): void {
    const target = event.target as HTMLTextAreaElement;
    if (
      target &&
      target.tagName.toLowerCase() === "textarea" &&
      target.classList.contains("code-area")
    ) {
      this.resizeTextArea(target);
    }
  }

  ngAfterViewInit(): void {
    this.resizeAll();
  }

  private resizeTextArea(textarea: HTMLTextAreaElement): void {
    textarea.style.height = "auto";
    textarea.style.height = `${textarea.scrollHeight}px`;
  }

  private resizeAll(): void {
    const areas: NodeListOf<HTMLTextAreaElement> =
      this.elRef.nativeElement.querySelectorAll("textarea.code-area");
    areas.forEach((ta) => this.resizeTextArea(ta));
  }

  back(): void {
    this.location.back();
  }
}
