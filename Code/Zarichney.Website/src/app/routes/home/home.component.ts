import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { LogoComponent } from '../../components/header/logo/logo.component';
import { LoggingService } from '../../services/log.service';
import { ResponsiveService } from '../../services/responsive.service';
import { SeoService } from '../../services/seo.service';

@Component({
  selector: 'home-screen',
  standalone: true,
  imports: [LogoComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeScreenComponent implements OnInit {
  
  constructor(
    private log: LoggingService,
    private responsiveService: ResponsiveService,
    private seoService: SeoService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    if (isPlatformBrowser(this.platformId)) {
      // Run browser-specific code here (e.g. afterNextRender)
    }
  }

  ngOnInit() {
    // Apply specific SEO settings for the home page
    this.seoService.setHomePageTags();
  }

  openResume() {
    if (isPlatformBrowser(this.platformId)) {
      window.open('/Resume_Steven_Zarichney.pdf', '_blank');
    }
  }
}