import { CommonModule, Location } from "@angular/common";
import { Component, DestroyRef, OnInit, inject } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormsModule } from "@angular/forms";
import { combineLatest, finalize } from "rxjs";
import { ContentDisplayComponent } from "../../components/content-display/content-display.component";
import { ContentKind } from "../../model/content-type";
import { ConvertRequest, SourceUnit } from "../../model/convert";
import { OrmTechnology } from "../../model/orm-type";
import {
  RequiredContentDefinition,
  RequiredContentUnit,
} from "../../model/required-content";
import { OrmService } from "../../services/orm.service";

interface EditableUnitState {
  content: string;
  language: string | null;
}

@Component({
  selector: "app-main-page",
  standalone: true,
  imports: [CommonModule, FormsModule, ContentDisplayComponent],
  templateUrl: "./main-page.component.html",
  styleUrls: ["./main-page.component.less"],
})
export class MainPageComponent implements OnInit {
  private destroyRef = inject(DestroyRef);

  isLoading = false;
  sourceOrmId: string | null = null;
  targetOrmId: string | null = null;
  convertedUnits: SourceUnit[] = [];
  error = "";

  orms: OrmTechnology[] = [];
  contentKinds = new Map<string, ContentKind>();
  requiredContent: RequiredContentDefinition[] = [];
  displayUnits: RequiredContentUnit[] = [];
  contentState: Record<number, EditableUnitState> = {};
  samples: Map<number, string> = new Map();

  constructor(private ormService: OrmService, private location: Location) {}

  ngOnInit(): void {
    combineLatest([
      this.ormService.getOrmTechnologies(),
      this.ormService.getContentKinds(),
      this.ormService.getRequiredContent(),
      this.ormService.getSamples(),
    ])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(([orms, kinds, required, samples]) => {
        this.orms = orms;
        this.contentKinds = new Map(kinds.map((kind) => [kind.id, kind]));
        this.requiredContent = required;
        this.samples = new Map(
          Object.entries(samples).map(([k, v]) => [Number(k), v as string])
        );

        this.sourceOrmId = this.resolveDefaultSourceOrm();
        this.targetOrmId = this.resolveDefaultTargetOrm();
        this.updateRequiredUnits();
      });
  }

  private resolveDefaultSourceOrm(): string | null {
    const preferred = this.orms.find((o) => o.id === "ef-core");
    return preferred?.id ?? this.orms[0]?.id ?? null;
  }

  private resolveDefaultTargetOrm(): string | null {
    if (this.orms.length === 0) {
      return null;
    }

    const preferred = this.orms.find((o) => o.id === "dapper");
    if (preferred && preferred.id !== this.sourceOrmId) {
      return preferred.id;
    }

    const alternative = this.orms.find((o) => o.id !== this.sourceOrmId);
    return alternative?.id ?? this.sourceOrmId;
  }

  onSourceOrmChange(newOrm: string) {
    this.sourceOrmId = newOrm;
    this.updateRequiredUnits();
  }

  onTargetOrmChange(newOrm: string) {
    this.targetOrmId = newOrm;
    this.convertedUnits = [];
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
        this.contentState[unit.id] = {
          content: "",
          language: defaultLanguage,
        };
      } else if (!this.contentState[unit.id].language) {
        this.contentState[unit.id].language = defaultLanguage;
      }
    });
  }

  convert(): void {
    if (!this.sourceOrmId || !this.targetOrmId) {
      return;
    }

    this.isLoading = true;
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
    this.ormService
      .convert(body)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => (this.isLoading = false))
      )
      .subscribe({
        next: (r) => {
          this.convertedUnits = r.sources;
        },
        error: (err) => (this.error = err.message ?? "Conversion failed"),
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
  }

  updateContent(unit: RequiredContentUnit, value: string) {
    this.ensureState(unit.id, unit.contentKindId);
    this.contentState[unit.id].content = value;
  }

  updateLanguage(unit: RequiredContentUnit, language: string) {
    this.ensureState(unit.id, unit.contentKindId);
    this.contentState[unit.id].language = language;
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

  back(): void {
    this.location.back();
  }
}
