export type ArticleStatus = 'draft' | 'review' | 'published';
export type DifficultyLevel = 'beginner' | 'intermediate' | 'advanced';
export type AIProviderName = 'openai' | 'claude' | 'gemini';

export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  parentId?: string;
  iconUrl?: string;
  articleCount: number;
}

export interface Topic {
  id: string;
  title: string;
  slug: string;
  categoryId: string;
  category?: Category;
  summary?: string;
  difficultyLevel: DifficultyLevel;
  articleCount: number;
}

export interface Tag {
  id: string;
  name: string;
  slug: string;
  color?: string;
}

export interface Article {
  id: string;
  topicId: string;
  topic?: Topic;
  title: string;
  slug: string;
  content: string;
  summary?: string;
  status: ArticleStatus;
  readingTimeMinutes: number;
  viewCount: number;
  aiGenerated: boolean;
  aiProvider?: AIProviderName;
  seoTitle?: string;
  seoDescription?: string;
  seoKeywords?: string[];
  tags: Tag[];
  publishedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ArticleSummary {
  id: string;
  title: string;
  slug: string;
  summary?: string;
  readingTimeMinutes: number;
  viewCount: number;
  tags: Tag[];
  topic?: Pick<Topic, 'id' | 'title' | 'slug'>;
  publishedAt?: string;
}

export interface SearchResult {
  articles: ArticleSummary[];
  topics: Pick<Topic, 'id' | 'title' | 'slug' | 'summary'>[];
  totalCount: number;
}
