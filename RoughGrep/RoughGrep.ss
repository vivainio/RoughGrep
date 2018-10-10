; binary, argv, workdir

; launcher that runs in dos prompt
(define make-shell-launcher 
    (lambda (cmd) 
        (make-runner "cmd.exe" 
            (sprintf "/c {0}" (list cmd))
            ""            
        )
    )
)

(define robot-test-runner 
    (make-shell-launcher "pybot [[file]]"))
    
(add-command ".*.robot" robot-test-runner)
