import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { RouterModule } from "@angular/router";

import { AppComponent } from "./app.component";
import { HomeComponent } from "./home/home.component";
import { FetchDataComponent } from "./fetch-data/fetch-data.component";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { MatToolbarModule } from "@angular/material/toolbar";
import { MatButtonModule } from "@angular/material/button";
import { MatMenuModule } from "@angular/material/menu";
import { MatInputModule } from "@angular/material/input";
import { AppFormsModule } from "./forms/forms.module";
import { SignUpFormComponent } from "./forms/sign-up-form/sign-up-form.component";
import { MatIconModule } from "@angular/material/icon";
import { AuthManager } from "./services/auth/AuthManager";
import { JWTAuthInterceptor } from "./services/http/interceptors/JWTAuthInterceptor";
import { APIBaseURLInterceptor } from "./services/http/interceptors/APIBaseURLInterceptor";


export function getBaseUrl()
{
  return document.getElementsByTagName("base")[0].href;
}
const providers = [
  {
    provide: "BASE_URL",
    useFactory: getBaseUrl,
    deps: []
  },
  {
    provide: HTTP_INTERCEPTORS,
    useClass: APIBaseURLInterceptor,
    multi: true,
    deps: []
  },
  {
    provide: HTTP_INTERCEPTORS,
    useClass: JWTAuthInterceptor,
    multi: true,
    deps: [AuthManager]
  }
];

@NgModule({
  declarations: [
    AppComponent,
    FetchDataComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
    HttpClientModule,
    AppFormsModule,
    RouterModule.forRoot([
      { path: "", component: HomeComponent, pathMatch: "full" },
      { path: "signup", component: SignUpFormComponent, pathMatch: "full" }
    ]),
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatMenuModule,
    MatInputModule,
    MatIconModule
  ],
  providers: providers,
  bootstrap: [AppComponent]
})
export class AppModule { }
