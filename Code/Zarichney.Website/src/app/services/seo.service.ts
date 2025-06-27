import { Injectable } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

interface MetaTagDefinition {
  name?: string;
  property?: string;
  content: string;
}

@Injectable({
  providedIn: 'root'
})
export class SeoService {
  constructor(
    private meta: Meta,
    private title: Title
  ) { }

  updateTitle(pageTitle: string) {
    this.title.setTitle(pageTitle);
  }

  updateMetaTags(tags: MetaTagDefinition[]) {
    tags.forEach(tag => {
      if (tag.name) {
        this.meta.updateTag({ name: tag.name, content: tag.content });
      } else if (tag.property) {
        this.meta.updateTag({ property: tag.property, content: tag.content });
      }
    });
  }

  updateDescriptionMetaTag(description: string) {
    this.meta.updateTag({ name: 'description', content: description });
  }

  updateKeywordsMetaTag(keywords: string) {
    this.meta.updateTag({ name: 'keywords', content: keywords });
  }

  updateCanonicalURL(url: string) {
    let canonicalLink = document.querySelector("link[rel='canonical']");
    if (!canonicalLink) {
      canonicalLink = document.createElement('link');
      canonicalLink.setAttribute('rel', 'canonical');
      document.head.appendChild(canonicalLink);
    }
    canonicalLink.setAttribute('href', url);
  }


  setDefaultTags() {
    this.updateTitle('Steven Zarichney - System Developer & Engineer');
    this.updateMetaTags([
      { name: 'description', content: 'Steven Zarichney is a System Developer/Engineer with expertise in system analysis, integration, and AI-driven innovation. Based in North Bay, Ontario, Canada.' },
      { name: 'keywords', content: 'Steven Zarichney, System Developer, Engineer, Software Development, System Integration, AI, Machine Learning, North Bay, Ontario' },
      { name: 'author', content: 'Steven Zarichney' },

      // Open Graph tags
      { property: 'og:title', content: 'Steven Zarichney - System Developer & Engineer' },
      { property: 'og:description', content: 'Steven Zarichney is a System Developer/Engineer with expertise in system analysis, integration, and AI-driven innovation.' },
      { property: 'og:type', content: 'website' },
      { property: 'og:url', content: 'https://zarichney.com' },
      { property: 'og:site_name', content: 'Steven Zarichney' },

      // Twitter Card tags
      { name: 'twitter:card', content: 'summary' },
      { name: 'twitter:title', content: 'Steven Zarichney - System Developer & Engineer' },
      { name: 'twitter:description', content: 'Steven Zarichney is a System Developer/Engineer with expertise in system analysis, integration, and AI-driven innovation.' },

      // Additional information for bots/crawlers
      { name: 'robots', content: 'index, follow' },
      { name: 'googlebot', content: 'index, follow' }
    ]);
    this.updateCanonicalURL('https://zarichney.com'); // Set default canonical
  }

  setHomePageTags() {
    this.updateTitle('Custom Software & AI Development Services | Steven Zarichney - North Bay, ON'); // More service-focused title
    this.updateDescriptionMetaTag('Need custom software or AI solutions? Steven Zarichney, a skilled System Developer & Engineer in North Bay, Ontario, offers expert web application, API, and AI development services. Contact us for a free consultation.'); // Improved description
    this.updateKeywordsMetaTag('Custom Software Development, Web Application Development, API Integration, AI Application Development, North Bay Software Developer, Ontario Engineer, System Integration, Software Modernization'); // Refined keywords
    this.updateMetaTags([ // Keep using updateMetaTags for other meta that are less frequently updated
      // Professional information for crawlers (consider if these are really needed as meta tags - Schema is better)
      { name: 'profession', content: 'System Developer/Engineer' },
      { name: 'location', content: 'North Bay, Ontario, Canada' },
      { name: 'skills', content: 'System Integration, AI, Machine Learning, Full-Stack Development, Software Architecture' },
      { name: 'languages', content: 'English, French' },
      { name: 'education', content: 'Bachelor of Computer Science with Honors, Laurentian University' },

      // Open Graph specific to home page
      { property: 'og:title', content: 'Custom Software & AI Development Services | Steven Zarichney - North Bay, ON' },
      { property: 'og:url', content: 'https://zarichney.com' },
      { property: 'og:description', content: 'Need custom software or AI solutions? Steven Zarichney, a skilled System Developer & Engineer in North Bay, Ontario, offers expert web application, API, and AI development services. Contact us for a free consultation.' }
    ]);
    this.updateCanonicalURL('https://zarichney.com'); // Ensure canonical URL is set for homepage too.
  }

  setServicePageTags(serviceName: string, serviceDescription: string, keywords: string, serviceUrlPath: string) {
    const pageTitle = `${serviceName} Services | Steven Zarichney - North Bay, ON`; // Example: Web Application Development Services | ...
    this.updateTitle(pageTitle);
    this.updateDescriptionMetaTag(serviceDescription);
    this.updateKeywordsMetaTag(keywords);
    this.updateMetaTags([
      { property: 'og:title', content: pageTitle },
      { property: 'og:description', content: serviceDescription },
      { property: 'og:url', content: `https://zarichney.com/${serviceUrlPath}` }, // Dynamic service URL
    ]);
    this.updateCanonicalURL(`https://zarichney.com/${serviceUrlPath}`); // Dynamic canonical URL
  }
}