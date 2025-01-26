<template>
  <Swiper
    :loop="true"
    :autoplay="{ delay: 3000, disableOnInteraction: false }"
    effect="fade"
    :fadeEffect="{ crossFade: true }"
    :pagination="{ el: '.swiper-pagination', clickable: true }"
    :navigation="{ nextEl: '.swiper-button-next', prevEl: '.swiper-button-prev' }"
    :speed="600"
    :slidesPerView="1"
  >
    <SwiperSlide>
      <div class="product-card">
        <div class="images-grid">
          <img src="https://via.placeholder.com/150" alt="Product Image 1">
          <img src="https://via.placeholder.com/150" alt="Product Image 2">
          <img src="https://via.placeholder.com/150" alt="Product Image 3">
          <img src="https://via.placeholder.com/150" alt="Product Image 4">
          <img src="https://via.placeholder.com/150" alt="Product Image 5">
        </div>
        <div class="info">
          <h3>Product Name</h3>
          <p class="price">$99.99</p>
        </div>
      </div>
    </SwiperSlide>
    <SwiperSlide>
      <div class="product-card">
        <div class="images-grid">
          <img src="https://via.placeholder.com/150" alt="Product Image 1">
          <img src="https://via.placeholder.com/150" alt="Product Image 2">
          <img src="https://via.placeholder.com/150" alt="Product Image 3">
          <img src="https://via.placeholder.com/150" alt="Product Image 4">
          <img src="https://via.placeholder.com/150" alt="Product Image 5">
        </div>
        <div class="info">
          <h3>Product Name</h3>
          <p class="price">$99.99</p>
        </div>
      </div>
    </SwiperSlide>
    <SwiperSlide>
      <div class="product-card">
        <div class="images-grid">
          <img src="https://via.placeholder.com/150" alt="Product Image 1">
          <img src="https://via.placeholder.com/150" alt="Product Image 2">
          <img src="https://via.placeholder.com/150" alt="Product Image 3">
          <img src="https://via.placeholder.com/150" alt="Product Image 4">
          <img src="https://via.placeholder.com/150" alt="Product Image 5">
        </div>
        <div class="info">
          <h3>Product Name</h3>
          <p class="price">$99.99</p>
        </div>
      </div>
    </SwiperSlide>

    <!-- Add Pagination -->
    <div class="swiper-pagination"></div>
    <!-- Add Navigation -->
    <div class="swiper-button-next"></div>
    <div class="swiper-button-prev"></div>
  </Swiper>
</template>


<script>
import { Swiper, SwiperSlide } from 'swiper/vue';
import 'swiper/swiper-bundle.css';  // Используем только этот импорт стилей

export default {
  components: {
    Swiper,
    SwiperSlide
  }
}
</script>

<style scoped>
.swiper-container {
  justify-content: center;
  align-items: start;
  width: 100%;
  height: 500px;
}

.swiper-slide {
  display: flex;
  align-items: center;
  justify-content: center;
  background: #333;
  color: #fff;
  font-size: 24px;
  height: 500px;
  border-radius: 15px; /* Закругленные углы */
  margin: 0 auto; /* Центровка */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2); /* Мягкая тень */
  transition: transform 0.3s ease-in-out;
}

.swiper-slide:hover {
  transform: scale(1.05); /* Легкое увеличение при наведении */
}

.product-card {
  position: relative;
  width: 100%;
  height: 100%;
  border-radius: 15px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
}

.images-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  grid-template-rows: repeat(3, 1fr);
  gap: 5px;
  flex-grow: 1;
}

.images-grid img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.info {
  background: linear-gradient(to top, rgba(0, 0, 0, 0.7), rgba(0, 0, 0, 0));
  padding: 10px;
  color: white;
  width: 100%;
  text-align: center;
}

.price {
  position: absolute;
  bottom: 10px;
  right: 10px;
  background: rgba(0, 0, 0, 0.7);
  padding: 5px 10px;
  border-radius: 5px;
}

/* Скрываем кнопки навигации на мобильной версии */
@media (max-width: 768px) {
  .swiper-button-next,
  .swiper-button-prev {
    display: none;
  }
}

/* Уменьшаем ширину карточек на ПК и центрируем их */
@media (min-width: 769px) {
  .swiper-slide {
    margin-top: 2%;
    margin-bottom: 2%;
    margin-right: 15%;
    margin-left: 15%;
    max-width: 70%; /* Уменьшаем ширину на ПК */
  }
}
</style>
