import type { AIProviderName } from './article.js';

export type AIJobType =
  | 'generate_article'
  | 'summarize'
  | 'generate_pdf'
  | 'generate_tags'
  | 'generate_seo'
  | 'generate_outline'
  | 'validate_content';

export type AIJobStatus = 'pending' | 'processing' | 'done' | 'failed';

export interface AIJob {
  id: string;
  jobType: AIJobType;
  status: AIJobStatus;
  provider: AIProviderName;
  promptTokens?: number;
  completionTokens?: number;
  costUsd?: number;
  errorMessage?: string;
  createdAt: string;
  completedAt?: string;
}

export interface KnowledgeRelation {
  id: string;
  sourceTopicId: string;
  targetTopicId: string;
  relationType: 'prerequisite' | 'related' | 'extends' | 'contradicts';
  strength: number;
}

export interface PdfDocument {
  id: string;
  articleId: string;
  fileUrl: string;
  fileSizeBytes: number;
  exportStyle: 'academic' | 'student' | 'presentation' | 'summary';
  generatedAt: string;
  downloadCount: number;
}
