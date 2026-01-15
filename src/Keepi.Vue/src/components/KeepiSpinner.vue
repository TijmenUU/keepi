<script setup lang="ts">
import { useDark } from '@vueuse/core'

const isDark = useDark()
</script>

<template>
  <div v-if="isDark" class="loader-white"></div>
  <div v-else class="loader-black"></div>
</template>

<style scoped>
.loader-white {
  height: 15px;
  aspect-ratio: 4;
  --_g: no-repeat radial-gradient(farthest-side, #eee 90%, #0000);
  background:
    var(--_g) left,
    var(--_g) right;
  background-size: 25% 100%;
  display: grid;
}
.loader-white:before,
.loader-white:after {
  content: '';
  height: inherit;
  aspect-ratio: 1;
  grid-area: 1/1;
  margin: auto;
  border-radius: 50%;
  transform-origin: -100% 50%;
  background: #eee;
  animation: l49 1s infinite linear;
}
.loader-white:after {
  transform-origin: 200% 50%;
  --s: -1;
  animation-delay: -0.5s;
}

.loader-black {
  height: 15px;
  aspect-ratio: 4;
  --_g: no-repeat radial-gradient(farthest-side, #000 90%, #0000);
  background:
    var(--_g) left,
    var(--_g) right;
  background-size: 25% 100%;
  display: grid;
}
.loader-black:before,
.loader-black:after {
  content: '';
  height: inherit;
  aspect-ratio: 1;
  grid-area: 1/1;
  margin: auto;
  border-radius: 50%;
  transform-origin: -100% 50%;
  background: #000;
  animation: l49 1s infinite linear;
}
.loader-black:after {
  transform-origin: 200% 50%;
  --s: -1;
  animation-delay: -0.5s;
}

@keyframes l49 {
  58%,
  100% {
    transform: rotate(calc(var(--s, 1) * 1turn));
  }
}
</style>
