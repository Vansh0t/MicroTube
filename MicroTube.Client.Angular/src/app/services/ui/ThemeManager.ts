import { ApplicationRef, Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";

@Injectable({
  providedIn: "root"
})
export class ThemeManager
{
  private readonly LOCAL_STORAGE_THEME_KEY = "prefered_theme";
  readonly theme = new BehaviorSubject<"light-theme" | "dark-theme">("light-theme");
  constructor(private ref: ApplicationRef)
  {
    this.theme.subscribe(this.applyThemeToHTML.bind(this));
    let preferedTheme = this.getThemeFromLocalStorage();
    if (preferedTheme)
    {
      this.theme.next(preferedTheme);
    }
    else
    {
      preferedTheme = this.getHTMLTheme();
      this.theme.next(preferedTheme);
      window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", this.onHTMLThemeChanged.bind(this));

    }
  }
  toggleTheme()
  {
    const newTheme = this.theme.value === "dark-theme" ? "light-theme" : "dark-theme";
    this.saveThemePreferenceToLocalStorage(newTheme);
    this.theme.next(newTheme);
  }
  private getThemeFromLocalStorage(): "light-theme" | "dark-theme" | null
  {
    const value = localStorage.getItem(this.LOCAL_STORAGE_THEME_KEY);
    if (value == "dark-theme" || value == "light-theme")
    {
      return value;
    }
    return null;
  }
  private saveThemePreferenceToLocalStorage(theme: "light-theme" | "dark-theme")
  {
    localStorage.setItem(this.LOCAL_STORAGE_THEME_KEY, theme);
  }
  private getHTMLTheme()
  {
    const prefersDarkTheme = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
    if (prefersDarkTheme)
    {
      return "dark-theme";
    }
    return "light-theme";
  }
  private onHTMLThemeChanged(event: MediaQueryListEvent)
  {
    if (this.getThemeFromLocalStorage())
    {
      return;
    }
    const turnOn = event.matches;
    this.theme.next(turnOn ? "dark-theme" : "light-theme");

    // trigger refresh of UI
    this.ref.tick();
  }
  private applyThemeToHTML(theme: "light-theme" | "dark-theme")
  {
    const bodyClasslist = document.body.classList;
    bodyClasslist.remove("light-theme");
    bodyClasslist.remove("dark-theme");
    bodyClasslist.add(theme);
  }
}
