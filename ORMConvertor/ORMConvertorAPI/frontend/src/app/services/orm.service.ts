import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ConvertRequest, ConvertResponse } from "../model/convert";
import { ContentKind } from "../model/content-type";
import { OrmTechnology } from "../model/orm-type";
import { RequiredContentDefinition } from "../model/required-content";

@Injectable({ providedIn: "root" })
export class OrmService {
  constructor(private http: HttpClient) { }

  private base = "/api";

  getRequiredContent(): Observable<RequiredContentDefinition[]> {
    return this.http.get<RequiredContentDefinition[]>(`${this.base}/required-content`);
  }

  getRequiredContentAdvisor(): Observable<RequiredContentDefinition[]> {
    return this.http.get<RequiredContentDefinition[]>(`${this.base}/required-content-advisor`);
  }

  getOrmTechnologies(): Observable<OrmTechnology[]> {
    return this.http.get<OrmTechnology[]>(`${this.base}/orm-technologies`);
  }

  getContentKinds(): Observable<ContentKind[]> {
    return this.http.get<ContentKind[]>(`${this.base}/content-kinds`);
  }

  getSamples(): Observable<Record<number, string>> {
    return this.http.get<Record<number, string>>(`${this.base}/samples`);
  }

  convert(req: ConvertRequest): Observable<ConvertResponse> {
    return this.http.post<ConvertResponse>(`${this.base}/convert`, req);
  }
}
