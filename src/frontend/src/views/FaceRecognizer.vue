<template>
  <div class="p-4 max-w-md mx-auto space-y-4">
    <h1 class="text-xl font-semibold">Face Recognizer</h1>

    <!-- Имя -->
    <input
        v-model="name"
        type="text"
        placeholder="Введите имя"
        class="w-full px-4 py-2 border rounded"
    />

    <!-- Загрузка фото -->
    <div class="space-y-2">
      <input type="file" accept="image/*" @change="handleFileUpload" />
    </div>

    <!-- Предпросмотр -->
    <div v-if="previewUrl" class="mt-4 space-y-2">
      <h2 class="text-lg font-medium">Предпросмотр:</h2>
      <img :src="previewUrl" class="w-full rounded" />

      <button
          @click="submit"
          class="w-full px-4 py-2 bg-indigo-600 text-white rounded"
          :disabled="!name || !imageBlob"
      >
        Отправить
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const name = ref('')
const imageBlob = ref(null)
const previewUrl = ref(null)

const handleFileUpload = (event) => {
  const file = event.target.files[0]
  if (file) {
    imageBlob.value = file
    previewUrl.value = URL.createObjectURL(file)
  } else {
    imageBlob.value = null
    previewUrl.value = null
  }
}

const submit = async () => {
  const formData = new FormData()
  formData.append('name', name.value)
  formData.append('image', imageBlob.value)

  const response = await fetch('http://localhost:5000/api/facerecognizer/addface', {
    method: 'POST',
    body: formData,
  })

  if (response.ok) {
    alert('Отправлено!')
    // Можно очистить форму, если нужно:
    name.value = ''
    imageBlob.value = null
    previewUrl.value = null
  } else {
    alert('Ошибка при отправке')
  }
}
</script>
