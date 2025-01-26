<template>
  <div class="hero">
    <div class="container">
      <h2>Welcome to Clothing Store</h2>
      <p>Discover the latest trends in fashion</p>
      <div class="buttons">
        <a class="waves-effect waves-light btn catalog-btn" @click="goToCatalog">Catalog</a>
        <a class="waves-effect waves-light btn login-btn" @click="login">Login</a>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'HeroBlock',
  methods: {
    goToCatalog() {
      // Переход в каталог
    },
    login() {
      // Вход
    }
  }
}
</script>

<style scoped>
.hero {
  background-image: url('https://via.placeholder.com/1500x500');
  background-size: cover;
  background-position: center;
  width: 100vw;  /* Ширина на весь экран */
  height: 100vh;  /* Высота на весь экран */
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  color: white;
  position: relative;
}

.hero .container {
  position: relative;
  z-index: 1;
  width: 100%;
}

.hero::before {
  content: "";
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 0;
}

.buttons a {
  margin: 0.5rem;
  border-radius: 25px; /* Закругление кнопки */
  transition: all 0.2s ease-in-out;
}

.buttons a:hover {
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.3); /* Тень при наведении */
}

.buttons a:active {
  background-color: #555; /* Изменение цвета при нажатии */
  transform: scale(0.9); /* Уменьшение при клике */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
  animation: pulse 0.4s ease-in-out;
}

@keyframes pulse {
  50% {
    transform: scale(1.1); /* Увеличение после клика */
  }
  100% {
    transform: scale(1); /* Возвращение к нормальному размеру */
  }
}
</style>
