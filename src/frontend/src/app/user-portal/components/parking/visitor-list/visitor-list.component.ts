import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormArray } from '@angular/forms';

@Component({
  selector: 'app-visitor-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './visitor-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VisitorListComponent {
  @Input() visitorsFormArray!: FormArray;
  @Output() add = new EventEmitter<void>();
  @Output() remove = new EventEmitter<number>();
}
