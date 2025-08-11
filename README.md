# 🎬 MovieHouse

A modern and user-friendly movie management system. Discover films, rate them, and create your personal watchlists.

## ✨ Features

- 🎯 Movie Discovery: Search through 1,000+ movies
- ⭐ Rating System: 5-star movie rating system
- 📋 Personal Lists: Watched, to-watch, and custom lists
- 🔍 Advanced Search: Search by movie title, director, actor, and genre
- 🎨 Modern UI: Responsive and user-friendly interface
- 👤 User System: Sign up and manage your personal profile

## 🚀 Quick Start
### Requirements

    .NET 8.0 SDK

    Python 3.8+

    SQL Server LocalDB

### Installation

1. **Clone the repository:**

         git clone https://github.com/username/MovieHouse.git
         cd MovieHouse

2. **Automatic setup (Recommended):**

         setup_and_run.bat

3. **Manual setup:**

       # Install Python packages
       pip install -r requirements.txt

       # Create the database
       dotnet ef database update

       # Import data
       python import_movies.py

       # Run the project
       dotnet run

### Quick Run

Once the project is set up:

      run.bat

## 📁 Project Structure

      MovieHouse/
      ├── Controllers/          # MVC Controllers
      ├── Models/               # Data models
      ├── Views/                # Razor views
      ├── Data/                 # Entity Framework context
      ├── wwwroot/              # Static files (CSS, JS)
      ├── Migrations/           # Database migrations
      ├── Datasets2/            # Movie datasets
      ├── import_movies.py      # Data import script
      ├── setup_and_run.bat     # Automatic setup
      └── run.bat               # Quick run script

## 🛠️ Technologies

   **Backend**: ASP.NET Core MVC

   **Database**: SQL Server LocalDB

   **ORM**: Entity Framework Core

   **Frontend**: HTML5, CSS3, JavaScript, Bootstrap

   **Data Processing**: Python (pandas, pyodbc)

   **Web Scraping**: BeautifulSoup, requests

## 📊 Database Schema

   **Films**: Movie information

   **Users**: User accounts

   **UserLists**: User watchlists

   **UserRatings**: Movie ratings

   **Categories**: Movie categories

   **Directors**: Directors

   **Actors**: Actors

## 🎯 Usage

   **Sign Up**: Create a new account

   **Discover Movies**: Browse movies on the homepage

   **Search**: Use the search bar

   **Rate**: Rate movies with 5 stars

   **Create Lists**: Manage your personal movie lists

## 🔧 Development
### Add a New Migration

      dotnet ef migrations add MigrationName
      dotnet ef database update

### Reset the Database

      dotnet ef database drop --force
      dotnet ef database update

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (git checkout -b feature/AmazingFeature)
3. Commit your changes (git commit -m 'Add some AmazingFeature')
4. Push to the branch (git push origin feature/AmazingFeature)
5. Create a Pull Request

## 📞 Contact

For any questions about the project, feel free to open an issue.
---
**MovieHouse** – A modern platform for movie lovers 🎬
