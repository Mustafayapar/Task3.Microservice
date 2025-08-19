

## 🚀 Proje Hakkında

Bu proje, **Onion Mimarisi** ve **Mikroservis yaklaşımı** ile
geliştirilmiş bir backend uygulamasıdır.\
Amaç; **SOLID prensiplerine** ve **12 Faktör Uygulama metodolojisine**
uygun, ölçeklenebilir bir mimari oluşturmaktır.

Proje kapsamında 3 mikro servis bulunmaktadır:\
- **Auth Service** → JWT tabanlı kimlik doğrulama, refresh token
yönetimi\
- **Product Service** → CQRS, Redis Cache ve event-driven yapı ile ürün
yönetimi\
- **Log Service** → Merkezi loglama, Serilog + JSON formatında
structured logging

Ayrıca:\
- **API Gateway (YARP)** kullanılarak servisler tek giriş noktasında
toplanmıştır.\
- **Rate Limiting**, **Caching**, **Event-driven mimari (RabbitMQ)**
entegrasyonları yapılmıştır.

------------------------------------------------------------------------

## ⚙️ Kurulum

### Gereksinimler

-   .NET 8 SDK\
-   SQL Server\
-   Redis (cache için)\
-   RabbitMQ (event-driven mimari için)

### Adımlar

1.  Repoyu klonlayın:

    ``` bash
    git clone https://github.com/<kullanici-adi>/<repo-adi>.git
    cd <repo-adi>
    ```

2.  Bağımlılıkları yükleyin:

    ``` bash
    dotnet restore
    ```

3.  **Veritabanı migration işlemi** (ilk çalıştırmadan önce):

    ``` bash
    cd src/Services/Auth/Auth.Api
    dotnet ef database update
    ```

4.  **Servisleri çalıştırın** (Visual Studio veya CLI üzerinden):

    ``` bash
    dotnet run --project src/Services/Auth/Auth.Api
    dotnet run --project src/Services/Product/Product.Api
    dotnet run --project src/Services/Log/Log.Api
    dotnet run --project src/ApiGateway
    ```

------------------------------------------------------------------------

## ▶️ Çalıştırma

-   **Gateway üzerinden erişim**:

        https://localhost:5000

-   Swagger dokümantasyonu her mikroserviste aktif:

        https://localhost:{port}/swagger

------------------------------------------------------------------------

## 🔄 Dağıtım Süreci

-   Geliştirmeler `test/v1.0.0` branch üzerinde yapılır.\
-   Stabil sürüm için `prod/v1.0.0` branch'e merge edilir.\
-   CI/CD süreçleri (opsiyonel) ile production ortamına dağıtım
    yapılabilir.

------------------------------------------------------------------------

## 📂 Kod Deposu

👉 [GitHub Repository
Linki](https://github.com/Mustafayapar/Task3.Microservice)
