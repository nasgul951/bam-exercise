import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AstronautDutyService } from '../../../core/services/astronaut-duty.service';
import { PersonAstronaut } from '../../../shared/models/person.model';
import { AstronautDuty } from '../../../shared/models/astronaut-duty.model';

@Component({
  selector: 'app-person-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <button mat-button (click)="back()" class="mb-4">
      <mat-icon>arrow_back</mat-icon> Back
    </button>

    @if (loading) {
      <div class="flex justify-center p-8"><mat-spinner /></div>
    } @else if (person) {
      <mat-card class="mb-6">
        <mat-card-header>
          <mat-card-title>{{ person.name }}</mat-card-title>
          <mat-card-subtitle>
            {{ person.currentRank || 'No rank' }} · {{ person.currentDutyTitle || 'No duty' }}
          </mat-card-subtitle>
        </mat-card-header>
        <mat-card-content class="pt-2 grid grid-cols-2 gap-2">
          <div><span class="font-medium">Career Start:</span> {{ person.careerStartDate ? (person.careerStartDate | date:'mediumDate') : '—' }}</div>
          <div><span class="font-medium">Career End:</span> {{ person.careerEndDate ? (person.careerEndDate | date:'mediumDate') : '—' }}</div>
        </mat-card-content>
      </mat-card>

      <h2 class="text-xl font-semibold mb-2">Astronaut Duties</h2>
      <div class="overflow-x-auto shadow rounded mb-6">
        <table mat-table [dataSource]="duties" class="w-full">
          <ng-container matColumnDef="rank">
            <th mat-header-cell *matHeaderCellDef>Rank</th>
            <td mat-cell *matCellDef="let d">{{ d.rank }}</td>
          </ng-container>
          <ng-container matColumnDef="dutyTitle">
            <th mat-header-cell *matHeaderCellDef>Duty Title</th>
            <td mat-cell *matCellDef="let d">{{ d.dutyTitle }}</td>
          </ng-container>
          <ng-container matColumnDef="dutyStartDate">
            <th mat-header-cell *matHeaderCellDef>Start Date</th>
            <td mat-cell *matCellDef="let d">{{ d.dutyStartDate | date:'mediumDate' }}</td>
          </ng-container>
          <ng-container matColumnDef="dutyEndDate">
            <th mat-header-cell *matHeaderCellDef>End Date</th>
            <td mat-cell *matCellDef="let d">{{ d.dutyEndDate ? (d.dutyEndDate | date:'mediumDate') : '—' }}</td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="dutyColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: dutyColumns;"></tr>
        </table>
        @if (duties.length === 0) {
          <p class="p-4 text-gray-500">No duties recorded.</p>
        }
      </div>

      <mat-card>
        <mat-card-header>
          <mat-card-title>Add New Duty</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="dutyForm" (ngSubmit)="submitDuty()" class="grid grid-cols-2 gap-4 pt-2">
            <mat-form-field appearance="outline">
              <mat-label>Rank</mat-label>
              <input matInput formControlName="rank" maxlength="50" />
              @if (dutyForm.get('rank')?.hasError('required') && dutyForm.get('rank')?.touched) {
                <mat-error>Rank is required</mat-error>
              }
              @if (dutyForm.get('rank')?.hasError('maxlength')) {
                <mat-error>Rank cannot exceed 50 characters</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Duty Title</mat-label>
              <input matInput formControlName="dutyTitle" maxlength="100" />
              @if (dutyForm.get('dutyTitle')?.hasError('required') && dutyForm.get('dutyTitle')?.touched) {
                <mat-error>Duty title is required</mat-error>
              }
              @if (dutyForm.get('dutyTitle')?.hasError('maxlength')) {
                <mat-error>Duty title cannot exceed 100 characters</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Start Date</mat-label>
              <input matInput [matDatepicker]="picker" formControlName="dutyStartDate" />
              <mat-datepicker-toggle matIconSuffix [for]="picker" />
              <mat-datepicker #picker />
              @if (dutyForm.get('dutyStartDate')?.hasError('required') && dutyForm.get('dutyStartDate')?.touched) {
                <mat-error>Start date is required</mat-error>
              }
            </mat-form-field>

            <div class="flex items-center gap-2">
              <button mat-raised-button color="primary" type="submit" [disabled]="submitting">
                @if (submitting) {
                  <mat-spinner diameter="18" class="inline-block mr-1" />
                }
                Add Duty
              </button>
            </div>

            @if (dutyError) {
              <p class="col-span-2 text-red-600 text-sm">{{ dutyError }}</p>
            }
          </form>
        </mat-card-content>
      </mat-card>
    }
  `,
})
export class PersonDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dutyService = inject(AstronautDutyService);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  person: PersonAstronaut | null = null;
  duties: AstronautDuty[] = [];
  dutyColumns = ['rank', 'dutyTitle', 'dutyStartDate', 'dutyEndDate'];
  loading = false;
  submitting = false;
  dutyError = '';
  private personName = '';

  dutyForm = this.fb.group({
    rank: ['', [Validators.required, Validators.maxLength(50)]],
    dutyTitle: ['', [Validators.required, Validators.maxLength(100)]],
    dutyStartDate: [null as Date | null, Validators.required],
  });

  ngOnInit(): void {
    this.personName = this.route.snapshot.paramMap.get('name') ?? '';
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.dutyService.getDutiesByName(this.personName).subscribe({
      next: (result) => {
        this.loading = false;
        this.person = result.person;
        this.duties = result.astronautDuties ?? [];
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to load person details', 'Close', { duration: 4000 });
      },
    });
  }

  submitDuty(): void {
    if (this.dutyForm.invalid) {
      this.dutyForm.markAllAsTouched();
      return;
    }
    this.submitting = true;
    this.dutyError = '';
    const { rank, dutyTitle, dutyStartDate } = this.dutyForm.value;
    const payload = {
      name: this.personName,
      rank: rank!,
      dutyTitle: dutyTitle!,
      dutyStartDate: (dutyStartDate as Date).toISOString(),
    };
    this.dutyService.createDuty(payload).subscribe({
      next: () => {
        this.submitting = false;
        this.snackBar.open('Duty added successfully', 'Close', { duration: 3000 });
        this.dutyForm.reset();
        this.loadData();
      },
      error: (err) => {
        this.submitting = false;
        this.dutyError = err?.error?.message ?? 'Failed to add duty';
      },
    });
  }

  back(): void {
    this.router.navigate(['/persons']);
  }
}
