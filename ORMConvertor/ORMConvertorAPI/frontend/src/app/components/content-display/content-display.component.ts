import {
  Component,
  EventEmitter,
  Input,
  Output,
  ChangeDetectionStrategy,
  AfterViewInit,
  OnChanges,
  SimpleChanges,
  ViewChild,
  ElementRef,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
@Component({
  selector: "app-content-display",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule],
  templateUrl: "./content-display.component.html",
  styleUrls: ["./content-display.component.less"],
})
export class ContentDisplayComponent implements AfterViewInit, OnChanges {
  @Input() title: string = "";
  @Input() languageOptions: string[] = [];
  @Input() language: string | null = null;
  @Input() content: string = "";
  @Input() autoResize: boolean = false;
  @Input() description: string = "";
  @Output() contentChange = new EventEmitter<string>();
  @Output() languageChange = new EventEmitter<string>();
  @Input() readonly = false;
  @ViewChild('codeArea') private codeArea!: ElementRef<HTMLTextAreaElement>;

  ngAfterViewInit(): void {
    if (this.autoResize) {
      this.resize();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.autoResize && changes['content']) {
      setTimeout(() => this.resize(), 0);
    }

    if (
      this.languageOptions?.length &&
      (!this.language || !this.languageOptions.includes(this.language))
    ) {
      const defaultLang = this.languageOptions[0];
      this.language = defaultLang;
      this.languageChange.emit(defaultLang);
    }
  }

  private resize(): void {
    if (!this.codeArea) {
      return;
    }
    const ta = this.codeArea.nativeElement;
    ta.style.height = 'auto';
    ta.style.height = `${ta.scrollHeight+25}px`;
  }
}
