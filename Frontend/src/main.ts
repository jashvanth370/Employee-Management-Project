import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';
import { initializeMsal } from '../src/app/service/msal-instance';

async function bootstrap() {
  await initializeMsal(); // ensures MSAL is ready
  await bootstrapApplication(AppComponent, appConfig);
}

bootstrap().catch(err => console.error(err));
