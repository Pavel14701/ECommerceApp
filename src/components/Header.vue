<template>
  <nav class="nav-wrapper">
    <div>
      <a href="#" class="brand-logo">Clothing Store</a>
      <ul id="nav-mobile" class="right hide-on-med-and-down">
        <li><a href="#">Home</a></li>
        <li><a href="#">Shop</a></li>
        <li><a href="#">About</a></li>
        <li><a href="#">Contact</a></li>
        <li><a href="#">Login</a></li>
        <li><a href="#">Register</a></li>
      </ul>
    </div>
  </nav>
</template>

<script>
export default {
  name: 'AppHeader'
}
</script>

<style scoped>
.nav-wrapper {
  background-color: black; /* Цвет фона хедера */
}

.brand-logo {
  padding-left: 20px;
}

</style>
