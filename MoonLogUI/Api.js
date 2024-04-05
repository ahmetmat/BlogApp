<script>
document.addEventListener("DOMContentLoaded", function() {
    fetch('https://localhost:7208/api/Blog')
    .then(response => response.json())
    .then(data => {
        const postsContainer = document.querySelector('.row'); // Blog postlarının ekleneceği container
        data.forEach(post => {
            const postDate = new Date(post.datePublished).toLocaleDateString("en-US");
            const postElement = `
                <div class="col-lg-6">
                    <div class="card mb-4">
                        <a href="#!"><img class="card-img-top" src="https://via.placeholder.com/700x350" alt="..."></a>
                        <div class="card-body">
                            <div class="small text-muted">${postDate}</div>
                            <h2 class="card-title h4">${post.title}</h2>
                            <p class="card-text">${post.content}</p>
                            <a class="btn btn-primary" href="#!">Read more →</a>
                        </div>
                    </div>
                </div>
            `;
            postsContainer.innerHTML += postElement; // Her bir postu container'a ekleyin
        });
    })
    .catch(error => console.error('Error:', error))};
);
</script>
