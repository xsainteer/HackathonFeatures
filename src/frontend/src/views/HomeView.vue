<template>
  <div class="container">
    <h1>Menu</h1>
    <div class="buttons">
      <router-link to="/FaceRecognizer" class="btn">FaceRecognizer</router-link>
      <router-link to="/FormRecognizer" class="btn">FormRecognizer</router-link>
      <router-link to="/FraudDetection" class="btn">FraudDetection</router-link>
    </div>
  </div>
</template>

<style scoped>
.container {
  max-width: 600px;
  margin: 50px auto;
  text-align: center;
  font-family: Arial, sans-serif;
}

.buttons {
  display: flex;
  justify-content: space-around;
  margin-top: 30px;
  gap: 10px;
  flex-wrap: wrap;
}

.btn {
  display: inline-block;
  padding: 12px 24px;
  background: linear-gradient(135deg, #6b73ff, #000dff);
  color: white;
  text-decoration: none;
  font-weight: 600;
  border-radius: 8px;
  box-shadow: 0 4px 8px rgb(107 115 255 / 0.5);
  transition: background 0.3s ease, transform 0.2s ease;
}

.btn:hover {
  background: linear-gradient(135deg, #000dff, #6b73ff);
  transform: translateY(-3px);
}
</style>
