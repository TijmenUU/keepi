import type { ApiError } from '@/types'
import { toast } from 'vue-sonner'

let hasShownSessionExpired = false

export function handleApiError(error: ApiError) {
  if (error.type === 'unauthorized') {
    if (!hasShownSessionExpired) {
      hasShownSessionExpired = true
      toast('Je sessie is verlopen. Stel eventuele wijzigingen veilig en kies herlaad.', {
        dismissible: false,
        closeButton: false,
        duration: Infinity,
        action: {
          label: 'Herlaad',
          onClick: () => (location.href = '/signin'),
        },
      })
    }

    return
  }

  // This could happen if the user entity is somehow evicted from the database
  // unexpectedly yet the session/login is valid.
  if (error.type === 'forbidden') {
    toast.error('Je hebt onvoldoende bevoegdheden om deze actie uit te voeren.')
    return
  }

  toast.error('Een onverwachte fout is opgetreden en eventuele wijzigingen zijn niet opgeslagen.')
}
