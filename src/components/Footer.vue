<template>
  <footer class="page-footer black">
    <div class="container">
      <div class="row">
        <div class="col l3 s12">
          <h5 class="white-text">Clothing Store</h5>
          <p class="grey-text text-lighten-4">We offer the latest trends in fashion.</p>
        </div>
        <div class="col l2 s12">
          <h5 class="white-text">Links</h5>
          <ul>
            <li><a class="grey-text text-lighten-3" href="#!">Home</a></li>
            <li><a class="grey-text text-lighten-3" href="#!">Shop</a></li>
            <li><a class="grey-text text-lighten-3" href="#!">About</a></li>
            <li><a class="grey-text text-lighten-3" href="#!">Contact</a></li>
          </ul>
        </div>
        <div class="col l2 s12">
          <h5 class="white-text">Legal</h5>
          <ul>
            <li><a class="grey-text text-lighten-3" href="#!">Privacy Policy</a></li>
            <li><a class="grey-text text-lighten-3" href="#!">Terms of Service</a></li>
          </ul>
        </div>
        <div class="col l2 s12">
          <h5 class="white-text">Social Media</h5>
          <ul>
            <li><a class="grey-text text-lighten-3" href="#!">Facebook</a></li>
            <li><a class="grey-text text-lighten-3" href="#!">Instagram</a></li>
            <li><a class="grey-text text-lighten-3" href="#!">Twitter</a></li>
            <li><a class="grey-text text-lighten-3" href="#!">LinkedIn</a></li>
          </ul>
        </div>
        <div class="col l3 s12">
          <img src="https://via.placeholder.com/100" alt="Brand Logo" class="footer-logo">
        </div>
      </div>
    </div>
    <div class="footer-copyright">
      <div class="container">
        &copy; 2025 Clothing Store. All rights reserved.
      </div>
    </div>
  </footer>
</template>

<script>
export default {
  name: 'AppFooter'
}
</script>

<style scoped>
.page-footer {
  padding: 2rem 0;
}

.footer-logo {
  width: 100px;
  height: auto;
}

.black {
  background-color: black;
}
</style>
