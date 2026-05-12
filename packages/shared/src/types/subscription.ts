import type { SubscriptionTier } from './user.js';

export type SubscriptionStatus =
  | 'active'
  | 'past_due'
  | 'canceled'
  | 'trialing'
  | 'incomplete';

export interface Subscription {
  id: string;
  userId: string;
  tier: SubscriptionTier;
  status: SubscriptionStatus;
  stripeSubscriptionId: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
}

export interface CheckoutSessionResponse {
  checkoutUrl: string;
  sessionId: string;
}

export interface PortalSessionResponse {
  portalUrl: string;
}

export interface PricingPlan {
  tier: SubscriptionTier;
  name: string;
  priceMonthly: number;
  features: string[];
  stripePriceId?: string;
}
