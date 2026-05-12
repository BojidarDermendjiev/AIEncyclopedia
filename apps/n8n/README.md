# n8n Workflows

This directory contains exported n8n workflow JSON files.

## Getting Started

1. Access n8n at http://localhost:5678 (dev) or https://n8n.yourdomain.com (prod)
2. Import workflow files from this directory via Settings → Import from File

## Planned Workflows

| Workflow | Description | Status |
|----------|-------------|--------|
| `article-generation.json` | Topic → Outline → Article → Tags → SEO metadata | Phase 2 |
| `pdf-generation.json` | Article → PDF render → Storage | Phase 2 |
| `summarization.json` | Article → Summary → Cache update | Phase 2 |
| `knowledge-graph.json` | New article → Extract relations → Update graph | Phase 3 |

## AI Pipeline Flow

```
Topic Input (webhook / scheduled)
    ↓
AI Research (choose provider: OpenAI / Claude / Gemini)
    ↓
Outline Generation
    ↓
Article Creation
    ↓
Fact Validation
    ↓
SEO Optimization
    ↓
POST to /api/v1/articles (internal)
    ↓
PDF Rendering trigger
    ↓
Publish
```
