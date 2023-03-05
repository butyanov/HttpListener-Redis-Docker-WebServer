window.onload=function(){
    const signInBtn = document.getElementById("signIn");
    const signUpBtn = document.getElementById("signUp");
    const firstForm = document.getElementById("form1");
    const secondForm = document.getElementById("form2");
    const container = document.querySelector(".container");

    if(signInBtn){
        signInBtn.addEventListener("click", () => {
            container.classList.remove("left-panel-active");
        });
    }
    if(signUpBtn){
        signUpBtn.addEventListener("click", () => {
            container.classList.add("left-panel-active");
        });
    }
    if(firstForm){
        firstForm.addEventListener("submit", (e) => e.preventDefault());
    }
    if(secondForm){
        secondForm.addEventListener("submit", (e) => e.preventDefault());
    }
}
