.PHONY: clean
clean:
	docker rm $$(docker ps -a --filter "status=exited" | grep hfs-charges-nightly-process | grep -oE "^[[:xdigit:]]+")
	docker rmi $$(docker images --filter "dangling=true" -q)

.PHONY: run
run:
	-docker-compose build hfs-charges-nightly-process && docker-compose run hfs-charges-nightly-process
	-make clean
