import { apiClient } from './api'

export type SubscriptionTier = 'Free' | 'Pro' | 'Enterprise'

export const subscriptionsService = {
  createCheckout: async (tier: SubscriptionTier, successUrl: string, cancelUrl: string) => {
    const { data } = await apiClient.post<{ success: boolean; data: string }>('/api/v1/subscriptions/checkout', {
      tier,
      successUrl,
      cancelUrl,
    })
    return data.data
  },
}
