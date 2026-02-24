import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders, HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {

  // ---------------- AUTH ----------------
  username: string = '';
  password: string = '';
  token: string | null = localStorage.getItem('token'); // ✅ load from storage

  signupUsername: string = '';
  signupPassword: string = '';
  signupMessage: string = '';
  showLogin: boolean = true;
  showSignup: boolean = false;
  loginMessage: string = '';

  // ---------------- WEATHER ----------------
  city: string = '';
  weather: any;
  error: string = '';

  private apiBase = 'http://localhost:5149/api'; // your backend URL

  constructor(private http: HttpClient, private cd: ChangeDetectorRef) {}

  // ---------------- SIGNUP ----------------
  signup() {
    const payload = {
      username: this.signupUsername,
      password: this.signupPassword
    };

    this.http.post(`${this.apiBase}/auth/signup`, payload, { responseType: 'text' })
      .subscribe({
        next: (res) => {
          this.signupMessage = res;
          // Redirect to login form after successful signup
          this.showLogin = true;
          this.showSignup = false;
          this.signupUsername = '';
          this.signupPassword = '';
        },
        error: (err) => {
          this.signupMessage = err.error;
        }
      });
  }

  // ---------------- LOGIN ----------------
  login() {
    this.http.post<any>(`${this.apiBase}/auth/login`, {
      username: this.username,
      password: this.password
    }).subscribe({
      next: (res) => {
        this.token = res.token;
        localStorage.setItem('token', this.token!); // ✅ store token
        this.loginMessage = "Login successful";
        this.error = '';
      },
      error: () => {
        this.loginMessage = "Invalid credentials";
      }
    });
  }

  // ---------------- LOGOUT ----------------
  logout() {
    this.token = null;
    localStorage.removeItem('token');
    this.weather = null;
    this.city = '';
  }

  // ---------------- WEATHER ----------------
  getWeather() {
    if (!this.city.trim()) {
      this.error = "Please enter a city name";
      this.weather = null;
      return;
    }

    if (!this.token) {
      alert("Please login first!");
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${this.token}`
    });

    this.http.get<any>(`${this.apiBase}/weather/${this.city}`, { headers })
      .subscribe({
        next: (res) => {
          this.weather = res;
          this.error = '';
          this.cd.detectChanges();
        },
        error: () => {
          alert('City not found or Unauthorized');
          this.weather = null;
        }
      });
  }

  // ---------------- TEMPERATURE CLASS ----------------
  getTemperatureClass(): string {
    if (!this.weather) return '';

    const temp = this.weather.temperature;

    if (temp >= 27) return 'hot';
    if (temp <= 10) return 'cold';
    return 'normal';
  }
}