import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersonService } from '../../../core/services/person.service';
import { PersonAstronaut } from '../../../shared/models/person.model';
import { AddPersonDialogComponent } from '../add-person-dialog/add-person-dialog.component';

@Component({
  selector: 'app-persons-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="flex justify-between items-center mb-4">
      <h1 class="text-2xl font-semibold">Persons</h1>
      <button mat-raised-button color="primary" (click)="openAddDialog()">
        <mat-icon>add</mat-icon> Add Person
      </button>
    </div>

    @if (loading) {
      <div class="flex justify-center p-8">
        <mat-spinner />
      </div>
    } @else {
      <div class="relative overflow-x-auto shadow rounded">
        <table mat-table [dataSource]="persons" class="w-full">
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let p">{{ p.name }}</td>
          </ng-container>
          <ng-container matColumnDef="currentRank">
            <th mat-header-cell *matHeaderCellDef>Current Rank</th>
            <td mat-cell *matCellDef="let p">{{ p.currentRank ?? '—' }}</td>
          </ng-container>
          <ng-container matColumnDef="currentDutyTitle">
            <th mat-header-cell *matHeaderCellDef>Duty Title</th>
            <td mat-cell *matCellDef="let p">{{ p.currentDutyTitle ?? '—' }}</td>
          </ng-container>
          <ng-container matColumnDef="careerStartDate">
            <th mat-header-cell *matHeaderCellDef>Career Start</th>
            <td mat-cell *matCellDef="let p">{{ p.careerStartDate ? (p.careerStartDate | date:'mediumDate') : '—' }}</td>
          </ng-container>
          <ng-container matColumnDef="careerEndDate">
            <th mat-header-cell *matHeaderCellDef>Career End</th>
            <td mat-cell *matCellDef="let p">{{ p.careerEndDate ? (p.careerEndDate | date:'mediumDate') : '—' }}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let p">
              <button mat-icon-button color="primary" (click)="viewDetail(p.name)" title="View details">
                <mat-icon>edit</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </div>

      <mat-paginator
        [length]="totalCount"
        [pageSize]="pageSize"
        [pageIndex]="pageIndex"
        [pageSizeOptions]="[5, 10, 25]"
        (page)="onPage($event)"
      />
    }
  `,
})
export class PersonsListComponent implements OnInit {
  private readonly personService = inject(PersonService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  displayedColumns = ['name', 'currentRank', 'currentDutyTitle', 'careerStartDate', 'careerEndDate', 'actions'];
  persons: PersonAstronaut[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  loading = false;

  ngOnInit(): void {
    this.loadPersons();
  }

  loadPersons(): void {
    this.loading = true;
    this.personService.getPersons(this.pageIndex + 1, this.pageSize).subscribe({
      next: (result) => {
        this.loading = false;
        this.persons = result.people?.items ?? [];
        this.totalCount = result.people?.totalCount ?? 0;
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to load persons', 'Close', { duration: 4000 });
      },
    });
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadPersons();
  }

  viewDetail(name: string): void {
    this.router.navigate(['/persons', name]);
  }

  openAddDialog(): void {
    const ref = this.dialog.open(AddPersonDialogComponent, { width: '400px' });
    ref.afterClosed().subscribe(result => {
      if (result) {
        this.pageIndex = 0;
        this.loadPersons();
      }
    });
  }
}
