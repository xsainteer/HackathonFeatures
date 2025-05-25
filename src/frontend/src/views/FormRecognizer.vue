<template>
  <div class="form-recognizer">
    <h2>Конвертация документа</h2>

    <form @submit.prevent="submitForm">
      <div>
        <label for="file">Выберите фото таблицы:</label><br />
        <input
            type="file"
            id="file"
            accept="image/*,application/pdf"
            @change="onFileChange"
            required
        />
      </div>

      <div style="margin-top: 1em;">
        <label for="format">Выберите формат экспорта:</label><br />
        <select id="format" v-model="selectedFormat" required>
          <option disabled value="">-- Выберите формат --</option>
          <option value="Csv">CSV</option>
          <option value="Excel">Excel</option>
          <option value="Pdf">PDF</option>
        </select>
      </div>

      <div style="margin-top: 1em;">
        <button type="submit" :disabled="!file || !selectedFormat || loading">
          {{ loading ? 'Обработка...' : 'Конвертировать' }}
        </button>
      </div>

      <div v-if="error" style="color: red; margin-top: 1em;">
        Ошибка: {{ error }}
      </div>
    </form>
  </div>
</template>

<script>
export default {
  name: "FormRecognizer",
  data() {
    return {
      file: null,
      selectedFormat: "",
      loading: false,
      error: null,
    };
  },
  methods: {
    onFileChange(event) {
      this.file = event.target.files[0] || null;
      this.error = null;
    },
    async submitForm() {
      if (!this.file || !this.selectedFormat) {
        this.error = "Пожалуйста, выберите файл и формат.";
        return;
      }

      this.loading = true;
      this.error = null;

      try {
        const formData = new FormData();
        formData.append("file", this.file);
        formData.append("format", this.selectedFormat);

        const response = await fetch('http://localhost:5000/api/DocumentIntelligence/convert', {
          method: "POST",
          body: formData,
        });

        if (!response.ok) {
          const text = await response.text();
          throw new Error(`Сервер вернул ошибку: ${response.status} - ${text}`);
        }

        // Получаем имя файла из заголовка или формируем своё
        const contentDisposition = response.headers.get("content-disposition");
        let filename = "converted-file";
        if (contentDisposition) {
          const match = contentDisposition.match(/filename="?(.+)"?/);
          if (match && match[1]) {
            filename = match[1];
          }
        } else {
          // добавим расширение на основе выбранного формата
          filename += this.selectedFormat.toLowerCase() === "excel" ? ".xlsx" :
              this.selectedFormat.toLowerCase() === "csv" ? ".csv" : ".pdf";
        }

        const blob = await response.blob();

        // Скачиваем файл
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
  },
};
</script>

<style scoped>
.form-recognizer {
  max-width: 400px;
  margin: auto;
  font-family: Arial, sans-serif;
}
button {
  padding: 0.5em 1em;
  font-size: 1em;
}
</style>
