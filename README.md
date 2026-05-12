# AI Encyclopedia Platform

A next-generation AI-powered educational encyclopedia platform. Generates high-quality educational content across many domains, produces downloadable PDFs, and provides semantic search, knowledge graphs, and personalized learning.

## Architecture

```
[React 19 + TypeScript]  →  [ASP.NET Core 8 API]  →  [PostgreSQL + Redis]
                                     ↓
                          [n8n AI Content Pipeline]
                                     ↓
                        [Stripe] + [Cloudflare CDN]
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 19, TypeScript, Vite, TailwindCSS, shadcn/ui, Zustand, TanStack Query |
| Backend | ASP.NET Core 8, Clean Architecture, CQRS/MediatR, FluentValidation |
| Database | PostgreSQL 16, Redis 7 |
| AI | OpenAI GPT-4o, Anthropic Claude, Google Gemini (multi-provider) |
| Automation | n8n workflows |
| Payments | Stripe |
| Infra | Docker Compose, Traefik, GitHub Actions |

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine + Compose
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for local backend dev)
- [Node.js 20+](https://nodejs.org/) (for local frontend dev)

### 1. Clone & Configure

```bash
git clone <repo-url>
cd BrainStorm
cp .env.example .env
# Edit .env with your actual API keys and secrets
```

### 2. Start with Docker Compose

```bash
cd infra
docker compose up -d
```

Services will be available at:
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000
- **API Docs (Swagger)**: http://localhost:5000/swagger
- **n8n**: http://localhost:5678
- **Traefik Dashboard**: http://localhost:8080

### 3. Run Database Migrations

```bash
docker compose exec api dotnet ef database update
```

### 4. Local Development

**Frontend:**
```bash
cd apps/web
npm install
npm run dev
```

**Backend:**
```bash
cd apps/api
dotnet restore
dotnet run --project src/AiEncyclopedia.API
```

## Project Structure

```
BrainStorm/
├── apps/
│   ├── api/          # ASP.NET Core 8 backend
│   ├── web/          # React 19 frontend
│   └── n8n/          # n8n workflow exports
├── infra/            # Docker, Traefik, CI/CD scripts
├── packages/
│   └── shared/       # Shared TypeScript types
├── .github/
│   └── workflows/    # GitHub Actions CI/CD
└── .env.example
```

## Subscription Tiers

| Feature | Free | Pro | Enterprise |
|---------|------|-----|------------|
| Read articles | ✓ | ✓ | ✓ |
| PDF downloads | 3/month | Unlimited | Unlimited |
| AI assistant | — | ✓ | ✓ |
| Saved library | — | ✓ | ✓ |
| API access | — | — | ✓ |
| Team features | — | — | ✓ |

## Development Phases

- **Phase 1** — Foundation: repo, infra, auth, DB schema, design system, CI/CD
- **Phase 2** — Core Platform: AI engine, PDF gen, search, subscriptions
- **Phase 3** — AI Intelligence: semantic search, knowledge graph, AI assistant
- **Phase 4** — Scale: SEO, i18n, programmatic content, CDN optimization
