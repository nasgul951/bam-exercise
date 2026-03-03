import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LogsService } from '../../../core/services/logs.service';
import { LogEntry } from '../../../shared/models/log-entry.model';

@Component({
  selector: 'app-logs-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="flex justify-between items-center mb-4">
      <h1 class="text-2xl font-semibold">Logs</h1>
    </div>

    @if (loading) {
      <div class="flex justify-center p-8"><mat-spinner /></div>
    } @else {
      <div class="overflow-x-auto shadow rounded">
        <table mat-table [dataSource]="logs" class="w-full">
          <ng-container matColumnDef="timestamp">
            <th mat-header-cell *matHeaderCellDef>Timestamp</th>
            <td mat-cell *matCellDef="let l">{{ l.timestamp | date:'short' }}</td>
          </ng-container>
          <ng-container matColumnDef="logLevel">
            <th mat-header-cell *matHeaderCellDef>Level</th>
            <td mat-cell *matCellDef="let l">
              <span [class]="getLevelClass(l.logLevel)" class="px-2 py-0.5 rounded text-xs font-semibold">
                {{ l.logLevel }}
              </span>
            </td>
          </ng-container>
          <ng-container matColumnDef="category">
            <th mat-header-cell *matHeaderCellDef>Category</th>
            <td mat-cell *matCellDef="let l">{{ l.category }}</td>
          </ng-container>
          <ng-container matColumnDef="message">
            <th mat-header-cell *matHeaderCellDef>Message</th>
            <td mat-cell *matCellDef="let l" class="max-w-md truncate">{{ l.message }}</td>
          </ng-container>
          <ng-container matColumnDef="exception">
            <th mat-header-cell *matHeaderCellDef>Exception</th>
            <td mat-cell *matCellDef="let l" class="max-w-xs truncate text-red-600">{{ l.exception ?? '—' }}</td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
        @if (logs.length === 0) {
          <p class="p-4 text-gray-500">No logs found.</p>
        }
      </div>

      <mat-paginator
        [length]="totalCount"
        [pageSize]="pageSize"
        [pageIndex]="pageIndex"
        [pageSizeOptions]="[10, 25, 50]"
        (page)="onPage($event)"
      />
    }
  `,
})
export class LogsListComponent implements OnInit {
  private readonly logsService = inject(LogsService);
  private readonly snackBar = inject(MatSnackBar);

  displayedColumns = ['timestamp', 'logLevel', 'category', 'message', 'exception'];
  logs: LogEntry[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  loading = false;

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.loading = true;
    this.logsService.getLogs(this.pageIndex + 1, this.pageSize).subscribe({
      next: (result) => {
        this.loading = false;
        this.logs = result.logs?.items ?? [];
        this.totalCount = result.logs?.totalCount ?? 0;
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to load logs', 'Close', { duration: 4000 });
      },
    });
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadLogs();
  }

  getLevelClass(level: string): string {
    switch (level?.toLowerCase()) {
      case 'information': return 'bg-blue-100 text-blue-800';
      case 'warning': return 'bg-amber-100 text-amber-800';
      case 'error': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}
