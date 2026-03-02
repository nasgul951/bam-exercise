import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { PersonService } from '../../../core/services/person.service';

@Component({
  selector: 'app-add-person-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <h2 mat-dialog-title>Add Person</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="flex flex-col gap-2 pt-2">
        <mat-form-field appearance="outline">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" maxlength="200" />
          @if (form.get('name')?.hasError('required') && form.get('name')?.touched) {
            <mat-error>Name is required</mat-error>
          }
          @if (form.get('name')?.hasError('maxlength')) {
            <mat-error>Name cannot exceed 200 characters</mat-error>
          }
        </mat-form-field>
        @if (errorMessage) {
          <p class="text-red-600 text-sm">{{ errorMessage }}</p>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" (click)="onSubmit()" [disabled]="loading">
        @if (loading) {
          <mat-spinner diameter="18" class="inline-block mr-1" />
        }
        Add
      </button>
    </mat-dialog-actions>
  `,
})
export class AddPersonDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly personService = inject(PersonService);
  private readonly dialogRef = inject(MatDialogRef<AddPersonDialogComponent>);
  private readonly snackBar = inject(MatSnackBar);

  loading = false;
  errorMessage = '';

  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.errorMessage = '';
    const name = this.form.value.name!;
    this.personService.createPerson(name).subscribe({
      next: () => {
        this.loading = false;
        this.snackBar.open('Person added successfully', 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err?.error?.message ?? 'Failed to add person';
      },
    });
  }
}
