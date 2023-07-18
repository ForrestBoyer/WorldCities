import { Component, OnInit } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { ActivatedRoute, Router } from "@angular/router";
import { FormGroup, FormControl, Validators } from "@angular/forms";

import { environment } from "./../../environments/environment";
import { City } from "./city";
import { Country } from "./../countries/country";

@Component({
    selector: "app-city-edit",
    templateUrl: "./city-edit.component.html",
    styleUrls: ["./city-edit.component.scss"],
})
export class CityEditComponent implements OnInit {
    title?: string;
    form!: FormGroup;
    city?: City;
    id?: number;
    countries?: Country[];

    constructor(
        private activatedRoute: ActivatedRoute,
        private router: Router,
        private http: HttpClient
    ) {}

    ngOnInit(): void {
        this.form = new FormGroup({
            name: new FormControl("", Validators.required),
            lat: new FormControl("", Validators.required),
            lon: new FormControl("", Validators.required),
            countryId: new FormControl("", Validators.required),
        });

        this.loadData();
    }

    loadData() {
        this.loadCountries();

        const idParam = this.activatedRoute.snapshot.paramMap.get("id");
        this.id = idParam ? +idParam : 0;

        if (this.id) {
            // EDIT MODE
            const url = environment.baseUrl + "api/cities/" + this.id;
            this.http.get<City>(url).subscribe(
                (result) => {
                    this.city = result;
                    this.title = "Edit - " + this.city.name;

                    this.form.patchValue(this.city);
                },
                (error) => console.error(error)
            );
        } else {
            // ADD MODE
            this.title = "Create a new City";
        }
    }

    loadCountries() {
        const url = environment.baseUrl + "api/countries";
        const params = new HttpParams()
            .set("pageIndex", "0")
            .set("pageSize", "9999")
            .set("sortColumn", "name");

        this.http.get<any>(url, { params }).subscribe(
            (result) => {
                this.countries = result.data;
            },
            (error) => console.error(error)
        );
    }

  onSubmit() {
        var city = this.id ? this.city : {} as City;
        if (city) { 
            city.name = this.form.controls["name"].value;
            city.lat = +this.form.controls["lat"].value;
            city.lon = +this.form.controls["lon"].value;
            city.countryId = +this.form.controls["countryId"].value;

            if (this.id) {
                // EDIT MODE
                var url = environment.baseUrl + "api/cities/" + city.id;
                this.http.put<City>(url, city).subscribe(
                    (result) => {
                        console.log("City " + city!.id + " has been updated.");
                        this.router.navigate(["/cities"]);
                    },
                    (error) => console.error(error)
                );
            } else {
                // ADD MODE
                var url = environment.baseUrl + "api/cities";
                this.http.post<City>(url, city).subscribe(
                    (result) => {
                        console.log("City" + result.id + " has been created.");
                        this.router.navigate(["/cities"]);
                    },
                    (error) => console.error(error)
                );
            }
        }
    }
}
