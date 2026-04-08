import { tryParseDutchDate } from '@/date'
import { createRule, defineRegleConfig, type Maybe } from '@regle/core'
import { isFilled, maxLength, required, withMessage } from '@regle/rules'

const dutchDate = createRule({
  validator(value: Maybe<string>) {
    if (!isFilled(value)) {
      return true
    }

    return tryParseDutchDate(value) != null
  },
  message: 'Geen geldige datum',
})

export const { useRegle: useKeepiRegle } = defineRegleConfig({
  rules: () => ({
    required: withMessage(required, 'Dit veld is verplicht'),
    maxLength: withMessage(
      maxLength,
      ({ _$value, $params: [max] }) => `Maximale aantal karakters is ${max}`,
    ),
    dutchDate,
  }),
})
