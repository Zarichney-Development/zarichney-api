import { Component, HostListener } from '@angular/core';
import { LoggingService } from '../../services/log.service';
import { ApiService } from '../../services/api.service';
import { ResponsiveService } from '../../services/responsive.service';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject, tap } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'helper',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './helper.component.html',
  styleUrl: './helper.component.scss'
})
export class HelperComponent {
  deviceType: string | undefined;
  adminMode = new BehaviorSubject<boolean>(true);

  constructor(private log: LoggingService, private apiService: ApiService, private responsiveService: ResponsiveService, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.getDeviceType();
    this.route.queryParamMap.subscribe(params => {
      this.adminMode.next(params.get('admin') != null);
    });

    this.adminMode.asObservable().pipe(
      tap(adminMode => {
        if (adminMode) {
          this.apiService.$message().subscribe(message => {
            console.log("api message response", message);
          });
          this.apiService.getTest().subscribe(response => {
            console.log("api test response", response);
          });
        }
      })
    ).subscribe();
  }

  @HostListener('window:resize')
  onResize() {
    this.getDeviceType();
  }

  getDeviceType(): void {
    if (this.deviceType !== this.responsiveService.deviceType) {
      this.deviceType = this.responsiveService.deviceType;
      this.log.info('deviceType', this.deviceType);
    }
  }
}
